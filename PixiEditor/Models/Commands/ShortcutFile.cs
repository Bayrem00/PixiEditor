﻿using Newtonsoft.Json;
using PixiEditor.Models.DataHolders;
using System.IO;

namespace PixiEditor.Models.Commands
{
    public class ShortcutFile
    {
        private readonly CommandController _commands;

        public string Path { get; }

        public ShortcutFile(string path, CommandController controller)
        {
            _commands = controller;
            Path = path;

            if (!File.Exists(path))
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                File.Create(Path).Dispose();
            }
        }

        public void SaveShortcuts()
        {
            OneToManyDictionary<KeyCombination, string> shortcuts = new();

            foreach (var shortcut in _commands.Commands.GetShortcuts())
            {
                foreach (var command in shortcut.Value.Where(x => x.Shortcut != x.DefaultShortcut))
                {
                    shortcuts.Add(shortcut.Key, command.InternalName);
                }
            }

            File.WriteAllText(Path, JsonConvert.SerializeObject(shortcuts));
        }

        public IEnumerable<KeyValuePair<KeyCombination, IEnumerable<string>>> LoadShortcuts() => LoadShortcuts(Path);

        public static IEnumerable<KeyValuePair<KeyCombination, IEnumerable<string>>> LoadShortcuts(string path) =>
            JsonConvert.DeserializeObject<IEnumerable<KeyValuePair<KeyCombination, IEnumerable<string>>>>(File.ReadAllText(path));
    }
}
