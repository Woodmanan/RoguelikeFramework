using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class AutomatedFileWizard
{

    [MenuItem("Tools/Dangerous/RebuildResources")]

    static void RebuildResources()

    {
        string templatePath = "Assets/Scripts/CustomEditor/FileTemplates/ResourceTemplate.txt";
        string path = "Assets/Scripts/Gameplay Datatypes/Resources.cs";


        //Write some text to the test.txt file

        StreamWriter writer = new StreamWriter(path, false);
        StreamReader reader = new StreamReader(templatePath);

        string[] names = Enum.GetNames(typeof(Resource));

        while (true)
        {
            string line = reader.ReadLine();
            if (line == null) break;
            writer.WriteLine(line);
            if (line == "    //AUTO VARIABLES")
            {
                writeNames(names, writer);
            } else if (line == "                //AUTO GET SWITCH")
            {
                writeGetNames(names, writer);
            }
            else if (line == "                //AUTO SET SWITCH")
            {
                writeSetNames(names, writer);
            }
            else if (line == "        //AUTO PLUS")
            {
                writeOperator(names, writer, "+");
            }
            else if (line == "        //AUTO MINUS")
            {
                writeOperator(names, writer, "-");
            }

        }

        reader.Close();
        writer.Close();

        Debug.Log("File rewritten!");

        AssetDatabase.Refresh();

    }

    static void writeNames(string[] names, StreamWriter writer)
    {
        string offset = "    ";
        foreach (string s in names)
        {
            writer.WriteLine(offset + "public int " + s.ToLower() + ";");
        }
    }

    static void writeOperator(string[] names, StreamWriter writer, string op)
    {
        string offset = "        ";
        foreach (string s in names)
        {
            writer.WriteLine($"{offset}r.{s.ToLower()} = a.{s.ToLower()} {op} b.{s.ToLower()};");
        }
    }

    static void writeGetNames(string[] names, StreamWriter writer)
    {
        string offset = "                ";
        for (int i = 0; i < names.Length; i++)
        {
            writer.WriteLine(offset + "case " + i + $": //RESOURCE.{names[i]}");
            writer.WriteLine(offset + "    return " + names[i].ToLower() + ";");
        }
        /*foreach (string s in names)
        {
            writer.WriteLine(offset + "case Resource." + s + ":");
            writer.WriteLine(offset + "    return " + s.ToLower() + ";");
        }*/
        writer.WriteLine(offset + "default:");
        writer.WriteLine(offset + "    return -1;");
    }

    static void writeSetNames(string[] names, StreamWriter writer)
    {
        string offset = "                ";
        for (int i = 0; i < names.Length; i++)
        {
            writer.WriteLine(offset + "case " + i + $": //RESOURCE.{names[i]}");
            writer.WriteLine(offset + "    " + names[i].ToLower() + " = value;");
            writer.WriteLine(offset + "    break;");
        }
        /*foreach (string s in names)
        {
            writer.WriteLine(offset + "case Resource." + s + ":");
            writer.WriteLine(offset + "    " + s.ToLower() +  " = value;");
            writer.WriteLine(offset + "    break;");
        }*/
    }
}


