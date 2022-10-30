using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Reflection;
using System.Collections;
using System.Text;

namespace JumpSquareGames.CommandLine
{
    public sealed class CommandLine : MonoBehaviour
    {
        public bool CommandLineEnabled { get; private set; }
        public static CommandLine Instance;
        private List<CommandRef> _commandAttributes = new List<CommandRef>();

        void Start()
        {
            Instance = this;
            var objs = GameObject.FindObjectsOfType<UnityEngine.Object>();

            foreach (var obj in objs)
            {
                var objType = obj.GetType();
                var commandAttributes = CommandAttribute.GetParamenters(objType);

                foreach (var commandAttribute in commandAttributes)
                {
                    if (commandAttribute.commandName != "")
                    {
                        _commandAttributes.Add(new CommandRef(obj, commandAttribute));
                    }
                }
            }
        }

        public void RunCommand(string command)
        {
            string[] commandContent = command.Split(' ');

            for (int i = 0; i < _commandAttributes.Count; i++)
            {
                var attr = _commandAttributes[i];

                if (commandContent[0] == attr.CommandAttributes.commandName)
                {
                    int wordCounter = 0;
                    List<object> args = new List<object>();

                    if (commandContent.Length == 0)
                        return;

                    string[] expandedContents = command.Replace('|', ' ').Split(' ');
                    for (int k = 1; k < expandedContents.Length; k++)
                    {
                        string commandChunk = expandedContents[k];
                        string stringIndicator = "'";

                        if (commandChunk.StartsWith(stringIndicator) && commandChunk.EndsWith(stringIndicator))
                        {
                            string result = commandChunk.Replace(stringIndicator, "");
                            args.Add(result);
                            wordCounter++;
                            continue;
                        }
                        else if (commandChunk.StartsWith(stringIndicator))
                        {
                            StringBuilder stringBuilder = new StringBuilder();
                            stringBuilder.Append(expandedContents[k]);
                            wordCounter++;
                            for (int chunkIndex = k + 1; chunkIndex < expandedContents.Length; chunkIndex++)
                            {
                                stringBuilder.Append(" " + expandedContents[chunkIndex]);
                                wordCounter++;
                                if (expandedContents[chunkIndex].EndsWith(stringIndicator))
                                {
                                    k = chunkIndex;
                                    break;
                                }
                            }

                            string result = stringBuilder.ToString().Replace(stringIndicator, "");
                            args.Add(result);
                            continue;
                        }

                        if (commandChunk.Contains(','))
                        {
                            string[] content = commandChunk.Split(',');

                            switch (content.Length)
                            {
                                case 2:
                                    Vector2 v2 = ParseToV2(content);
                                    args.Add(v2);
                                    break;
                                case 3:
                                    object v3 = ParseToV3(content);
                                    args.Add(v3);
                                    break;
                                case 4:
                                    Vector3 qv3 = ParseToV3(content);

                                    float x = 0;
                                    float.TryParse(content[2], out x);
                                    args.Add(new Quaternion(qv3.x, qv3.y, qv3.z, x));
                                    break;
                            }

                            continue;
                        }

                        if (commandChunk.IsNumber())
                        {
                            args.Add(double.Parse(GetRandomValue(command.Replace('.', ',')).ToString()));
                        }
                    }

                    //Handles all lists in commandline

                    for (int k = 0; k < commandContent.Length; k++)
                    {
                        if (commandContent[k].Contains('|'))
                        {
                            int count = commandContent[k].Split('|').Count();

                            var argVal = args[k].GetType();
                            var listType = typeof(List<>).MakeGenericType(argVal);
                            var contents = (IList)Activator.CreateInstance(listType);

                            for (int n = 0; n < count; n++)
                            {
                                object val = args[k - wordCounter + n];
                                contents.Add(val);
                            }

                            args.RemoveRange(k - wordCounter, count);

                            var param = attr.CommandAttributes.parameterInfos[k - wordCounter].ParameterType;
                            string paramText = param.ToString();

                            if (paramText.Contains("List"))
                            {
                                args.Insert(k - wordCounter, contents);
                            }
                            else
                            {
                                MethodInfo castMethod = this.GetType().GetMethod("ListToArray", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(IList) }, null).MakeGenericMethod(argVal);
                                object castedObject = castMethod.Invoke(null, new object[] { contents });
                                args.Insert(k - wordCounter, castedObject);
                            }
                        }

                    }

                    attr.CommandAttributes.methodInfo.Invoke(attr.Caller, args.ToArray());
                }
            }

            Vector3 ParseToV3(string[] v3Content)
            {
                float x = 0;
                float.TryParse(GetRandomValue(v3Content[0].Replace('.', ',')).ToString(), out x);
                float y = 0;
                float.TryParse(GetRandomValue(v3Content[1].Replace('.', ',')).ToString(), out y);
                float z = 0;
                float.TryParse(GetRandomValue(v3Content[2].Replace('.', ',')).ToString(), out z);
                int abv = 1;

                return new Vector3(x, y, z);
            }

            Vector2 ParseToV2(string[] v2Content)
            {
                float x = 0;
                float.TryParse(GetRandomValue(v2Content[0].Replace('.', ',')).ToString(), out x);
                float y = 0;
                float.TryParse(GetRandomValue(v2Content[1].Replace('.', ',')).ToString(), out y);

                return new Vector2(x, y);
            }

            float GetRandomValue(string text)
            {
                string[] content = text.Split('-');
                float val = 0;

                if (content.Length == 1)
                {
                    float.TryParse(text, out val);

                    return val;
                }

                float min = 0;
                float.TryParse(content[0].Replace('.', ','), out min);
                float max = 0;
                float.TryParse(content[1].Replace('.', ','), out max);

                return UnityEngine.Random.Range(min, max);
            }
        }

        private static T[] ListToArray<T>(IList o)
        {
            T[] arr = new T[o.Count];

            for (int i = 0; i < o.Count; i++)
            {
                arr[i] = (T)o[i];
            }

            return arr;
        }

        struct CommandRef
        {
            public UnityEngine.Object Caller;
            public (string commandName, MethodInfo methodInfo, ParameterInfo[] parameterInfos) CommandAttributes;

            public CommandRef(UnityEngine.Object caller, (string commandName, MethodInfo methodInfo, ParameterInfo[] parameterInfos) commandAttributes)
            {
                Caller = caller;
                CommandAttributes = commandAttributes;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class CommandAttribute : Attribute
    {
        public string CommandName;

        public CommandAttribute(string commandName)
        {
            CommandName = commandName;
        }

        public static List<(string commandName, MethodInfo methodInfo, ParameterInfo[] parameterInfos)> GetParamenters(Type type)
        {
            List<(string, MethodInfo, ParameterInfo[])> content = new List<(string, MethodInfo, ParameterInfo[])>();//("", null, new ParameterInfo[] {});
            MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Union(type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)).ToArray();//new List<MethodInfo>();

            for (int i = 0; i < methods.Length; i++)
            {
                var attributes = methods[i].GetCustomAttributes(typeof(CommandAttribute), false);

                for (int k = 0; k < attributes.Length; k++)
                {
                    var at = (CommandAttribute)attributes[k];
                    content.Add((at.CommandName, methods[i], methods[i].GetParameters()));
                }
            }

            return content;
        }
    } 
}