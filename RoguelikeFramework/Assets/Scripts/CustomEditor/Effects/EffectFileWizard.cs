using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

public class EffectFileWizard
{

    [MenuItem("Tools/Dangerous/Rebuild Effects")]

    static void RebuildEffects()
    {

        //Load in our definitions from the file
        EffectConnections declarations = AssetDatabase.LoadAssetAtPath<EffectConnections>("Assets/Scripts/CustomEditor/Effects/Effect Connections.asset");

        declarations.connections.Sort((a, b) => a.priority.CompareTo(b.priority));

        if (declarations == null)
        {
            Debug.LogError("AGH IT'S ALL ON FIRE WHAT DID YOU CHANGE");
            Debug.LogError("The EffectFileWizard is having trouble finding the EffectConnections at 'Assets/Scripts/CustomEditor/Effects/Effect Connections.asset'. Did you move this? Move it back for fix the wizard.");
            return;
        }

        Debug.Log($"Connections asset found. There are {declarations.connections.Count} connections total, which will be added to the files.");

        //Write the declarations into Monster.cs
        WriteConnections(declarations);

        WriteEffect(declarations);

        WriteTemplate(declarations);

        //Rebuild the class files for each type
        foreach (Type type in GetAllEffectTypes())
        {
            RebuildClassConnections(type, declarations);
        }


        //Reload the asset database, triggering a recompile and yelling at us if this didn't work right.
        Debug.Log("Finished all writes, reloading assets and recompiling.");
        AssetDatabase.Refresh();
    }

    static void RebuildClassConnections(Type type, EffectConnections declarations)
    {
        string name = type.Name + ".cs";
        string path = GetPathTo(name);

        Debug.Log($"Rebuilding {type.Name}.");

        {//Write the connection part!
            List<string> connectionText = new List<string>();
            connectionText.Add("    public override void Connect(Connections c)");
            connectionText.Add("    {");
            connectionText.Add("        connectedTo = c;");
            connectionText.Add("");

            foreach (Connection c in declarations.connections)
            {
                MethodInfo method = type.GetMethod(c.name);
                if ((method.DeclaringType == null) || method.DeclaringType != typeof(Effect))
                {
                    int priority = 100;
                    object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
                    if (attribute != null)
                    {
                        priority = ((PriorityAttribute)attribute).Priority;
                    }
                    else
                    {
                        attribute = type.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
                        if (attribute != null)
                        {
                            priority = ((PriorityAttribute)attribute).Priority;
                        }
                    }

                    connectionText.Add($"        c.{c.name}.AddListener({priority}, {c.name});");
                    connectionText.Add("");
                }
            }
            connectionText.Add("        OnConnection();");
            connectionText.Add("    }");


            FillInFile(path, "Connection", connectionText);
        }

        {//Write the disconnection!
            List<string> connectionText = new List<string>();
            connectionText.Add("    public override void Disconnect()");
            connectionText.Add("    {");
            connectionText.Add("        OnDisconnection();");
            connectionText.Add("");

            foreach (Connection c in declarations.connections)
            {
                MethodInfo method = type.GetMethod(c.name);
                if (method.DeclaringType != typeof(Effect))
                {
                    int priority = 100;
                    object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
                    if (attribute != null)
                    {
                        priority = ((PriorityAttribute)attribute).Priority;
                    }
                    else
                    {
                        attribute = type.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
                        if (attribute != null)
                        {
                            priority = ((PriorityAttribute)attribute).Priority;
                        }
                    }

                    connectionText.Add($"        connectedTo.{c.name}.RemoveListener({c.name});");
                    connectionText.Add("");
                }
            }

            connectionText.Add("        ReadyToDelete = true;");
            connectionText.Add("    }");


            FillInFile(path, "Disconnection", connectionText);
        }

    }

    static void FillInFile(string path, string header, List<string> toFill)
    {
        StreamReader reader = new StreamReader(path);
        string fullHead = "//BEGIN " + header.Trim().ToUpper();
        string fullEnd = "//END " + header.Trim().ToUpper();

        //Read output
        List<string> lines = new List<string>();
        while (true)
        {
            string line = reader.ReadLine();
            if (line == null) break;
            lines.Add(line);
        }
        reader.Close();

        //Write new input
        bool writing = true;
        StreamWriter writer = new StreamWriter(path, false);
        for (int i = 0; i < lines.Count; i++)
        {
            if (writing)
            {
                writer.WriteLine(lines[i]);
            }
            if (lines[i].Trim().Equals(fullHead))
            {
                writing = false;
                foreach (string s in toFill)
                {
                    writer.WriteLine(s);
                }
            }
            if (lines[i].Trim().Equals(fullEnd))
            {
                writing = true;
                writer.WriteLine(lines[i]);
            }
        }
        writer.Close();
    }

    static string GetPathTo(string filename)
    {
        string path = "Assets";
        var info = new DirectoryInfo(path);

        List<string> filesToModify = new List<string>();

        FileInfo[] files = info.GetFiles("*.*", SearchOption.AllDirectories);

        foreach (FileInfo f in files)
        {
            if (filename.Equals(f.Name))
            {
                string filePath = f.FullName;
                int length = filePath.Length - info.FullName.Length + path.Length;
                filePath = filePath.Substring(f.FullName.Length - length, length);
                return filePath;
            }
        }
        return "No File Found!";
    }

    static void WriteConnections(EffectConnections declarations)
    {
        string path = GetPathTo("Connections.cs");
        string templatePath = GetPathTo("ConnectionTemplate.txt");
        StreamReader reader = new StreamReader(templatePath);

        List<string> lines = new List<string>();
        while (true)
        {
            string line = reader.ReadLine();
            if (line == null) break;
            lines.Add(line);
        }
        reader.Close();

        StreamWriter writer = new StreamWriter(path, false);
        for (int i = 0; i < lines.Count; i++)
        {
            writer.WriteLine(lines[i]);
            if (lines[i].Equals("    //BEGIN AUTO EVENTS"))
            {
                //Write in the lines!
                foreach (Connection c in declarations.connections)
                {
                    writer.WriteLine($"    public OrderedEvent{TypeName(c)} {c.name} = new OrderedEvent{TypeName(c)}();");
                }

                writer.WriteLine();
            }
        }

        writer.Close();
        Debug.Log("Finished writing connections");
    }

    static void WriteEffect(EffectConnections declarations)
    {
        string path = GetPathTo("Effect.cs");
        string templatePath = GetPathTo("EffectTemplate.txt");
        StreamReader reader = new StreamReader(templatePath);

        List<string> lines = new List<string>();
        while (true)
        {
            string line = reader.ReadLine();
            if (line == null) break;
            lines.Add(line);
        }
        reader.Close();

        StreamWriter writer = new StreamWriter(path, false);
        for (int i = 0; i < lines.Count; i++)
        {
            writer.WriteLine(lines[i]);
            if (lines[i].Equals("        //AUTO VARIABLE"))
            {
                writer.WriteLine($"        int numConnections = {declarations.connections.Count};");
            }
            else if (lines[i].Equals("        //AUTO SETUP"))
            {
                writer.WriteLine();
                //Write in the lines!
                for (int j = 0; j < declarations.connections.Count; j++)
                {
                    WriteSetup(declarations.connections[j], writer, j);
                    writer.WriteLine();
                }
            }
            else if (lines[i].Equals("        //BEGIN AUTO CONNECT"))
            {
                writer.WriteLine();
                for (int j = 0; j < declarations.connections.Count; j++)
                {
                    writer.WriteLine($"        c.{declarations.connections[j].name}.AddListener(100, {declarations.connections[j].name});");
                }
            }
            else if (lines[i].Equals("        //BEGIN AUTO DISCONNECT"))
            {
                writer.WriteLine();
                for (int j = 0; j < declarations.connections.Count; j++)
                {
                    writer.WriteLine($"        connectedTo.{declarations.connections[j].name}.RemoveListener({declarations.connections[j].name});");
                }
            }
            else if (lines[i].Equals("    //AUTO DECLARATIONS"))
            {
                writer.WriteLine();
                for (int j = 0; j < declarations.connections.Count; j++)
                {
                    writer.WriteLine($"    public virtual void {declarations.connections[j].name}({TypesForFunction(declarations.connections[j])}) {{}}");
                }
            }
        }

        writer.Close();
        Debug.Log("Finished writing effect file");
    }

    static void WriteTemplate(EffectConnections declarations)
    {
        string path = GetPathTo("EffectTemplate.cs.txt");
        string templatePath = GetPathTo("EffectTemplateTemplate.txt");
        StreamReader reader = new StreamReader(templatePath);

        List<string> lines = new List<string>();
        while (true)
        {
            string line = reader.ReadLine();
            if (line == null) break;
            lines.Add(line);
        }
        reader.Close();

        StreamWriter writer = new StreamWriter(path, false);
        for (int i = 0; i < lines.Count; i++)
        {
            
            if (lines[i].Equals("    //AUTO CONNECTIONS"))
            {
                for (int j = 0; j < declarations.connections.Count; j++)
                {
                    writer.WriteLine($"    //{declarations.connections[j].description}");
                    writer.WriteLine($"    //public override void {declarations.connections[j].name}({TypesForFunction(declarations.connections[j])}) {{}}");
                    writer.WriteLine();
                }
            }
            else if (lines[i].Equals("    //BEGIN CONNECTION"))
            {
                writer.WriteLine(lines[i]);
                writer.WriteLine("    //TEMPORARY CONNECTION - THIS WILL BE AUTO-REMOVED ONCE EFFECTS ARE REBUILT.");

                //Create overfull connection function
                writer.WriteLine($"    public override void Connect(Connections c)");
                writer.WriteLine($"    {{");
                writer.WriteLine($"        connectedTo = c;");

                for (int j = 0; j < declarations.connections.Count; j++)
                {
                    writer.WriteLine($"        connectedTo.{declarations.connections[j].name}.AddListener(100, {declarations.connections[j].name});");
                }
                writer.WriteLine("        OnConnection();");
                writer.WriteLine("    }");
            }
            else if (lines[i].Equals("    //BEGIN DISCONNECTION"))
            {
                writer.WriteLine(lines[i]);
                writer.WriteLine("    //TEMPORARY DISCONNECTION - THIS WILL BE AUTO-REMOVED ONCE EFFECTS ARE REBUILT.");

                //Create overfull connection function
                writer.WriteLine($"    public override void Disconnect()");
                writer.WriteLine($"    {{");
                writer.WriteLine($"        OnDisconnection();");

                for (int j = 0; j < declarations.connections.Count; j++)
                {
                    writer.WriteLine($"        connectedTo.{declarations.connections[j].name}.RemoveListener({declarations.connections[j].name});");
                }

                writer.WriteLine($"        ReadyToDelete = true;");
                writer.WriteLine("    }");
            }
            else
            {
                writer.WriteLine(lines[i]);
            }
        }

        writer.Close();
        Debug.Log("Finished writing script template");
    }

    static void WriteSetup(Connection c, StreamWriter writer, int index)
    {
        string dashes = new string('-', 60);
        int numDashes = (dashes.Length - c.name.Length - 2) / 2;

        writer.WriteLine($"        //{dashes.Substring(0, numDashes)} {c.name} {dashes.Substring(0, numDashes)}");
        writer.WriteLine($"        method = ((ActionRef{TypeName(c)}) {c.name}).Method;");
        writer.WriteLine(@"        if (method.DeclaringType != typeof(Effect))
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                connections[index] = ((PriorityAttribute)attribute).Priority;
            }
            else
            {
                connections[index] = this.priority;
            }

        }
        else
        {
            connections[index] = -1;
        }".Replace("index", $"{index}"));


        writer.WriteLine("        //" + dashes);
    }

    static string TypeName(Connection c)
    {
        if (c.types.Count == 0)
        {
            return "";
        }
        string newName = "<";
        for (int i = 0; i < c.types.Count; i++)
        {
            newName += c.types[i].type;
            if (i == c.types.Count - 1)
            {
                newName += ">";
            }
            else
            {
                newName += ", ";
            }
        }
        return newName;
    }

    static string TypesForFunction(Connection c)
    {
        if (c.types.Count == 0)
        {
            return "";
        }
        string newName = "";
        for (int i = 0; i < c.types.Count; i++)
        {
            newName += $"ref {c.types[i].type} {c.types[i].name}";
            if (i != c.types.Count - 1)
            {
                newName += ", ";
            }
        }
        return newName;
    }

    public static List<Type> GetAllEffectTypes()
    {
        Type baseType = typeof(Effect);
        Assembly assembly = baseType.Assembly;

        return assembly.GetTypes().Where(t => t.IsSubclassOf(baseType)).ToList();
    }
}
