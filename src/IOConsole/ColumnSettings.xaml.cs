﻿using IOConsole.Serializable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Path = System.IO.Path;

namespace IOConsole;

/// <summary>
/// Interaction logic for ColumnSettings.xaml
/// </summary>
public partial class ColumnSettings : Window
{
    private static readonly JsonSerializerOptions Jso = new() { WriteIndented = true };
    private static readonly string PartColumnConfigFile = Path.Combine(MainWindow.DataDir, "part_column_config.json");

    public ColumnSettings()
    {
        InitializeComponent();

        Initialize();
        SetBindings();

        ConfirmButton.Click += ConfirmColumnSettings;
    }

    private void Initialize()
    {
        if (!File.Exists(PartColumnConfigFile))
        {
            var json = JsonSerializer.Serialize(new PartColumnConfig(), Jso);
            File.WriteAllText(PartColumnConfigFile, json);
        }

        var cfg = JsonSerializer.Deserialize<PartColumnConfig>(File.ReadAllText(PartColumnConfigFile));
        if (cfg is not null)
            DataContext = cfg;
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            var ithChild = VisualTreeHelper.GetChild(depObj, i);
            if (ithChild is T t)
                yield return t;

            foreach (var childOfChild in FindVisualChildren<T>(ithChild))
                yield return childOfChild;
        }
    }

    private void SetTextEvent(object sender, TextChangedEventArgs e)
    {
        var box = (TextBox)sender;

        // only allow numeric characters
        box.Text = Regex.Replace(box.Text, @"[^\d]+", string.Empty).TrimStart('0');
        box.CaretIndex = box.Text.Length;

        // attach TextBlocks
        var textBlocks = FindVisualChildren<TextBlock>(TextBoxesGrid).ToArray();
        if (box.Name.Contains("Column", StringComparison.Ordinal))
        {
            var textBlock = (
                from b in textBlocks
                where b.Name == box.Name.Replace("NumberBox", "Alpha")
                select b).First();

            textBlock.Text = $"({NumberToAlpha(int.Parse(box.Text))})";
        }
    }

    private static string NumberToAlpha(int number)
    {
        if (number > 26)
            return string.Empty;

        return Convert.ToChar(Convert.ToInt16('A') + number - 1).ToString();
    }

    private void SetBindings()
    {
        var dp = TextBox.TextProperty;
        var columnConfig = (PartColumnConfig)DataContext;
        var textBoxes = FindVisualChildren<TextBox>(TextBoxesGrid);
        foreach (var tb in textBoxes)
        {
            tb.SetBinding(dp, new Binding(tb.Name.Replace("NumberBox", string.Empty)) { Source = columnConfig });
            tb.TextChanged += SetTextEvent;
        }
    }

    private void ConfirmColumnSettings(object? sender, RoutedEventArgs args)
    {
        var cfg = (PartColumnConfig)DataContext;
        var json = JsonSerializer.Serialize(cfg, Jso);
        File.WriteAllText(PartColumnConfigFile, json);

        DialogResult = true;

        Close();
    }
}