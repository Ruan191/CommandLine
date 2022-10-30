using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace JumpSquareGames.CommandLine
{
    [RequireComponent(typeof(CommandLine))]
    public class CommandLineUI : MonoBehaviour
    {
        public bool CommandLineEnabled { get; private set; }
        private GameObject _canvas;
        private TMP_InputField _inputField;

        private List<string> _commandUsedLog = new List<string>();
        private int _commandSelectedIndex = 0;
        [SerializeField] private GameObject _textLogPrefab;
        [SerializeField] private Transform _container;

        void Start()
        {
            _canvas = transform.GetChild(0).gameObject;
            _inputField = _canvas.transform.GetChild(0).GetComponent<TMP_InputField>();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                CommandLineEnabled = CommandLineEnabled.Toggle();
                _canvas.SetActive(CommandLineEnabled);

                if (CommandLineEnabled)
                {
                    _inputField.Select();
                    _inputField.ActivateInputField();
                }
            }

            if (Input.GetKeyDown(KeyCode.Return) && CommandLineEnabled)
            {
                string text = _inputField.text;

                if (text == "")
                    return;
                try
                {
                    CommandLine.Instance.RunCommand(text);
                }
                catch
                {
                    ConsoleLogError("Failed to run the command: " + text);
                }
                _commandUsedLog.Add(text);
                _commandSelectedIndex = _commandUsedLog.Count - 1;
                _inputField.text = "";
                _inputField.Select();
                _inputField.ActivateInputField();
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                _inputField.text = _commandUsedLog[_commandSelectedIndex = Mathf.Clamp(++_commandSelectedIndex, 0, _commandUsedLog.Count - 1)];
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                _inputField.text = _commandUsedLog[_commandSelectedIndex = Mathf.Clamp(--_commandSelectedIndex, 0, _commandUsedLog.Count - 1)];
            }
        }

        int textNumber = 0;

        public void ConsoleLog(object content)
        {
            var textLog = SpawnTextLog();
            textLog.textarea.text = content.ToString();
        }

        public void ConsoleLog(object content, UnityEngine.Color textColor)
        {
            var textLog = SpawnTextLog();
            textLog.textarea.color = textColor;
            textLog.textarea.text = content.ToString();
        }

        public void ConsoleLogError(object content)
        {
            var textLog = SpawnTextLog();
            textLog.textarea.color = UnityEngine.Color.red;
            textLog.textarea.text = content.ToString();
        }

        public void ConsoleLogWarning(object content)
        {
            var textLog = SpawnTextLog();
            textLog.textarea.color = UnityEngine.Color.yellow;
            textLog.textarea.text = content.ToString();
        }

        private (TextMeshProUGUI numberText, TextMeshProUGUI textarea) SpawnTextLog()
        {
            var obj = Instantiate(_textLogPrefab, _container);
            var numberText = obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            var text = obj.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            numberText.text = textNumber++.ToString();
            return (numberText, text);
        }

        [Command("print")]
        private void Log(string text)
        {
            ConsoleLog(text);
        }
    } 
}
