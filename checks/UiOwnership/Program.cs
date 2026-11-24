using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Mono.Cecil;
using Mono.Cecil.Cil;

if (args.Length != 3)
    throw new ArgumentException("Expected local-import ZIP, installed game Managed directory, and BepInEx core directory.");

var paths = args.Skip(1).Select(Path.GetFullPath).ToArray();
var context = AssemblyLoadContext.Default;
context.Resolving += (_, name) =>
{
    var path = paths.Select(dir => Path.Combine(dir, name.Name + ".dll")).FirstOrDefault(File.Exists);
    return path == null ? null : context.LoadFromAssemblyPath(path);
};

// Unity's native object lifetime and activation require a running player.
// Supply just those calls in memory so the actual packaged postfix can run offline.
using var unity = AssemblyDefinition.ReadAssembly(Path.Combine(paths[0], "UnityEngine.CoreModule.dll"));
var module = unity.MainModule;
var gameObject = module.GetType("UnityEngine.GameObject");
var active = new FieldDefinition("TestActive", Mono.Cecil.FieldAttributes.Public, module.TypeSystem.Boolean);
var destroyed = new FieldDefinition("TestDestroyed", Mono.Cecil.FieldAttributes.Public, module.TypeSystem.Boolean);
gameObject.Fields.Add(active);
gameObject.Fields.Add(destroyed);
var unityObject = module.GetType("UnityEngine.Object");
ILProcessor Replace(MethodDefinition method)
{
    method.ImplAttributes = Mono.Cecil.MethodImplAttributes.IL;
    method.Body = new Mono.Cecil.Cil.MethodBody(method);
    return method.Body.GetILProcessor();
}
foreach (var op in unityObject.Methods.Where(m => m.Name is "op_Equality" or "op_Inequality"))
{
    var il = Replace(op);
    il.Emit(OpCodes.Ldarg_0);
    il.Emit(OpCodes.Ldarg_1);
    il.Emit(OpCodes.Ceq);
    if (op.Name == "op_Inequality") { il.Emit(OpCodes.Ldc_I4_0); il.Emit(OpCodes.Ceq); }
    il.Emit(OpCodes.Ret);
}
var setActive = Replace(gameObject.Methods.Single(m => m.Name == "SetActive"));
setActive.Emit(OpCodes.Ldarg_0);
setActive.Emit(OpCodes.Ldarg_1);
setActive.Emit(OpCodes.Stfld, active);
setActive.Emit(OpCodes.Ret);
var destroy = Replace(unityObject.Methods.Single(m => m.Name == "Destroy" && m.Parameters.Count == 1));
destroy.Emit(OpCodes.Ldarg_0);
destroy.Emit(OpCodes.Castclass, gameObject);
destroy.Emit(OpCodes.Ldc_I4_1);
destroy.Emit(OpCodes.Stfld, destroyed);
destroy.Emit(OpCodes.Ret);
using var memory = new MemoryStream();
unity.Write(memory);
memory.Position = 0;
var engine = context.LoadFromStream(memory);
// The game's Mono-targeted Harmony initializer cannot run on CoreCLR. These
// checks dispatch the postfixes directly and the closed-tooltip path needs no
// native label or frame delegate. Leave the mod assembly itself untouched.
using var harmony = AssemblyDefinition.ReadAssembly(Path.Combine(paths[1], "0Harmony.dll"));
var access = harmony.MainModule.GetType("HarmonyLib.AccessTools");
Replace(access.Methods.Single(m => m.IsConstructor && m.IsStatic)).Emit(OpCodes.Ret);
var fieldRef = access.Methods.Single(m => m.Name == "FieldRefAccess" && m.GenericParameters.Count == 2 &&
    m.Parameters.Count == 1 && m.Parameters[0].ParameterType.FullName == "System.String");
var fieldRefBody = Replace(fieldRef);
fieldRefBody.Emit(OpCodes.Ldnull);
fieldRefBody.Emit(OpCodes.Ret);
using var harmonyMemory = new MemoryStream();
harmony.Write(harmonyMemory);
harmonyMemory.Position = 0;
context.LoadFromStream(harmonyMemory);
var game = context.LoadFromAssemblyPath(Path.Combine(paths[0], "Assembly-CSharp.dll"));
using var archive = ZipFile.OpenRead(args[0]);
using var dll = archive.GetEntry("plugins/ProductionBottlenecks.dll").Open();
using var bytes = new MemoryStream();
dll.CopyTo(bytes);
bytes.Position = 0;
var mod = context.LoadFromStream(bytes);
var plugin = mod.GetType("ProductionBottlenecks.Plugin", true);
const BindingFlags fields = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
object Blank(Type type) => RuntimeHelpers.GetUninitializedObject(type);
var instance = Blank(plugin);
var count = 0;
void Check(string description, bool ok)
{
    if (!ok) throw new Exception(description);
    Console.WriteLine("PASS " + description);
    count++;
}

// Verify startup registers the same patch classes that we invoke below.
bytes.Position = 0;
using var metadata = AssemblyDefinition.ReadAssembly(bytes);
var awake = metadata.MainModule.GetType(plugin.FullName).Methods.Single(m => m.Name == "Awake");
var registrations = awake.Body.Instructions.Where(i => i.OpCode == OpCodes.Ldtoken &&
    i.Operand is TypeReference && i.Next?.Operand is MethodReference typeOf && typeOf.Name == "GetTypeFromHandle" &&
    i.Next.Next?.Operand is MethodReference patchAll && patchAll.Name == "PatchAll")
    .Select(i => ((TypeReference)i.Operand).Name).ToHashSet();

// Select postfixes by their real Harmony target metadata, including the overload.
var overlay = game.GetType("SuperFantasyKingdom.UI.UIOverlayVariable", true);
var hooks = new Dictionary<Type, MethodInfo>();
foreach (var patch in plugin.GetNestedTypes(BindingFlags.NonPublic))
{
    var attr = patch.CustomAttributes.SingleOrDefault(a => a.AttributeType.FullName == "HarmonyLib.HarmonyPatch");
    if (attr == null || attr.ConstructorArguments.Count != 3 || !Equals(attr.ConstructorArguments[0].Value, overlay)) continue;
    var types = ((IEnumerable<CustomAttributeTypedArgument>)attr.ConstructorArguments[2].Value).Select(a => (Type)a.Value).ToArray();
    Check("target exists on installed game: " + types[0].Name, overlay.GetMethod((string)attr.ConstructorArguments[1].Value, types) != null);
    Check("startup registers " + patch.Name, registrations.Contains(patch.Name));
    hooks.Add(types[0], patch.GetMethod("Postfix", fields));
}
var world = game.GetType("SuperFantasyKingdom.Clickable.BuildingWorld", true);
var building = game.GetType("SuperFantasyKingdom.Buildings.Building", true);
Check("world generator has an ownership hook", hooks.ContainsKey(world));
Check("city generator has an ownership hook", hooks.ContainsKey(building));

void Set(string field, object value) => plugin.GetField(field, fields).SetValue(instance, value);
object Get(string field) => plugin.GetField(field, fields).GetValue(instance);
Set("instance", instance);
var panel = Blank(overlay);
var otherPanel = Blank(overlay);
var row = Blank(engine.GetType("UnityEngine.GameObject", true));
var selected = Blank(game.GetType("SuperFantasyKingdom.Buildings.BuildingCity", true));
var label = Blank(plugin.GetField("label", fields).FieldType);
void Seed()
{
    Set("rowPanel", panel);
    Set("row", row);
    Set("label", label);
    Set("selected", selected);
    row.GetType().GetField("TestActive").SetValue(row, true);
    row.GetType().GetField("TestDestroyed").SetValue(row, false);
}
bool Active() => (bool)row.GetType().GetField("TestActive").GetValue(row);
bool Released() => Get("row") == null && Get("rowPanel") == null && Get("label") == null && Get("selected") == null &&
    !Active() && (bool)row.GetType().GetField("TestDestroyed").GetValue(row);
Seed();
hooks[world].Invoke(null, new[] { otherPanel });
Check("unrelated world panel preserves the city's row and selection", Get("row") == row && Get("selected") == selected && Active());
hooks[world].Invoke(null, new[] { panel });
Check("reused world panel releases the old city row without native Clean", Released());
hooks[world].Invoke(null, new[] { panel });
Check("another world refresh leaves ownership cleared", Released());
Seed();
hooks[building].Invoke(null, new[] { otherPanel, selected, false });
Check("replaced city generator on another panel preserves the row", Get("row") == row && Active());
hooks[building].Invoke(null, new[] { panel, selected, false });
Check("replaced city generator on the same panel releases the row", Released());
Seed();
hooks[building].Invoke(null, new[] { panel, Blank(building), true });
Check("ordinary non-city Generate releases the city row", Released());

var visibility = mod.GetType("ProductionBottlenecks.HoverVisibility", true);
var state = Activator.CreateInstance(visibility);
var observe = visibility.GetMethod("Observe", fields);
bool Dismissed(string reason, bool open) => (bool)observe.Invoke(state, new object[] { reason, open });
Check("initial empty tooltip may show Tree", !Dismissed("Needs Tree", false));
Check("open shortage can refresh", !Dismissed("Needs Tree", true));
Check("visible foreign card preserves open state", !Dismissed("Needs Tree", true));
Check("closing foreign card dismisses the same shortage", Dismissed("Needs Tree", false));
Check("repeated refresh keeps the dismissal", Dismissed("Needs Tree", false));
Check("a later native title cannot undo a dismissal", Dismissed("Needs Tree", true));
Check("new shortage after dismissal may appear", !Dismissed("Needs Ore", false));
Check("an open shortage starts tracking immediately", !Dismissed("Needs Ore", true));
Check("directly closing the shortage also dismisses it", Dismissed("Needs Ore", false));
Check("resuming work clears dismissal", !Dismissed(null, false));
Check("return of the same shortage after work resumes is visible", !Dismissed("Needs Ore", false));
state = Activator.CreateInstance(visibility);
Check("fresh focus may show the same shortage", !Dismissed("Needs Ore", false));
// Run the actual refresh after a card was visible, then closed on the same building.
var tipType = game.GetType("SuperFantasyKingdom.TooltipManager", true);
var tip = Blank(tipType);
tipType.GetField("Instance").SetValue(null, tip);
Set("hovered", selected);
Set("tooltipText", "Lumberyard\nNeeds Tree");
Dismissed("Needs Tree", true);
Set("hoverVisibility", state);
var refresh = plugin.GetMethod("UpdateTooltip", fields);
refresh.Invoke(instance, new object[] { "Needs Tree" });
refresh.Invoke(instance, new object[] { "Needs Tree" });
Check("two actual plugin refreshes leave the closed card hidden",
    !(bool)tipType.GetMethod("IsOpen").Invoke(tip, null) && Get("tooltipText") == null);
Console.WriteLine($"{count} packaged UI ownership checks passed.");
