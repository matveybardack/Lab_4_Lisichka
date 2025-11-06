using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClassLibraryBoolTable.Models;
using ClassLibraryBoolTable.Parser;
using ClassLibraryBoolTable.Service;
using ClassLibraryBoolTable.Util;

namespace WpfAppBoolTable
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Вкладка "По номеру"

        private void BuildTruthTable(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!InputN?.Value.HasValue == true || !InputNum?.Value.HasValue == true)
                {
                    MessageBox.Show("Заполните все параметры", "Ошибка");
                    return;
                }

                int n = (int)InputN.Value;
                long num = InputNum.Value ?? 0;

                var truthTable = new TruthTable(n);
                truthTable.BuildFromNumber(num);

                UpdateTruthTableGrid(truthTable);

                var values = truthTable.Table.Select(row => row.Result).ToArray();
                BinaryExplanation.Text = BitUtils.ToBinaryString(values);

                CountInfo.Text = $"Всего функций для {n} переменных: {Math.Pow(2, Math.Pow(2, n)):G}";

                // Очищаем старые DNF/KNF
                OutputDNF.Clear();
                OutputKNF.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private void BuildDNF(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!InputN?.Value.HasValue == true || !InputNum?.Value.HasValue == true)
                {
                    MessageBox.Show("Заполните все параметры", "Ошибка");
                    return;
                }

                int n = (int)InputN.Value;
                long num = InputNum.Value ?? 0;

                var truthTable = new TruthTable(n);
                truthTable.BuildFromNumber(num);

                string dnf = DnfGenerator.BuildDNF(truthTable.Table, truthTable.VariableNames);
                OutputDNF.Text = dnf;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка построения DNF: {ex.Message}", "Ошибка");
            }
        }

        private void BuildKNF(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!InputN?.Value.HasValue == true || !InputNum?.Value.HasValue == true)
                {
                    MessageBox.Show("Заполните все параметры", "Ошибка");
                    return;
                }

                int n = (int)InputN.Value;
                long num = InputNum.Value ?? 0;

                var truthTable = new TruthTable(n);
                truthTable.BuildFromNumber(num);

                string knf = KnfGenerator.BuildKNF(truthTable.Table, truthTable.VariableNames);
                OutputKNF.Text = knf;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка построения KNF: {ex.Message}", "Ошибка");
            }
        }

        private void CopyDNF_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(OutputDNF?.Text))
            {
                Clipboard.SetText(OutputDNF.Text);
                MessageBox.Show("DNF скопирован в буфер обмена", "Успех");
            }
        }

        private void CopyKNF_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(OutputKNF?.Text))
            {
                Clipboard.SetText(OutputKNF.Text);
                MessageBox.Show("KNF скопирован в буфер обмена", "Успех");
            }
        }

        private void Preset_Number_1(object sender, RoutedEventArgs e)
        {
            InputN.Value = 3;
            InputNum.Value = 11;
            BuildTruthTable(sender, e);
        }

        #endregion

        #region Вкладка "По формуле"

        private void ParseFormula_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string formula = InputFormula?.Text?.Trim();
                if (string.IsNullOrEmpty(formula))
                {
                    MessageBox.Show("Введите формулу", "Ошибка");
                    return;
                }

                // Получаем список переменных из формулы
                var tokens = Lexer.Lex(formula);
                var variables = tokens
                    .Where(t => t.Type == TokenType.Variable)
                    .Select(t => t.Value)
                    .Distinct()
                    .OrderBy(v => v)
                    .ToList();

                if (!variables.Any())
                {
                    MessageBox.Show("В формуле не найдены переменные", "Ошибка");
                    return;
                }

                int n = variables.Count;

                // Преобразуем в ОПЗ
                var rpnTokens = ParserRpn.ToRpn(tokens);

                // Создаем таблицу истинности
                var truthTable = new TruthTable(n);
                truthTable.VariableNames.Clear();
                truthTable.VariableNames.AddRange(variables);

                // Генерируем все комбинации и вычисляем функцию
                var allTuples = BitUtils.GenerateAllTuples(n);
                truthTable.Table.Clear();

                foreach (var tuple in allTuples)
                {
                    var variableValues = new Dictionary<string, bool>();
                    for (int i = 0; i < n; i++)
                    {
                        variableValues[variables[i]] = tuple[i];
                    }

                    bool result = ExpressionEvaluator.Evaluate(rpnTokens, variableValues);
                    truthTable.Table.Add(new TupleResult(tuple, result));
                }

                // Обновляем интерфейс
                UpdateFormulaTableGrid(truthTable);

                string dnf = DnfGenerator.BuildDNF(truthTable.Table, truthTable.VariableNames);
                string knf = KnfGenerator.BuildKNF(truthTable.Table, truthTable.VariableNames);

                FormulaDNF.Text = dnf;
                FormulaKNF.Text = knf;

                FormulaCountInfo.Text = $"Формула использует {n} переменных: {string.Join(", ", variables)}";

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка разбора формулы: {ex.Message}", "Ошибка");
            }
        }

        private void CopyFormulaDNF_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(FormulaDNF?.Text))
            {
                Clipboard.SetText(FormulaDNF.Text);
                MessageBox.Show("DNF скопирован в буфер обмена", "Успех");
            }
        }

        private void CopyFormulaKNF_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(FormulaKNF?.Text))
            {
                Clipboard.SetText(FormulaKNF.Text);
                MessageBox.Show("KNF скопирован в буфер обмена", "Успех");
            }
        }

        private void Preset_Formula_Implication(object sender, RoutedEventArgs e)
        {
            InputFormula.Text = "(x1 | x2) -> x3";
            ParseFormula_Click(sender, e);
        }

        #endregion

        #region Вкладка "Сравнение"

        private void CompareFunctions_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!A_n?.Value.HasValue == true || !A_num?.Value.HasValue == true ||
                    !B_n?.Value.HasValue == true || !B_num?.Value.HasValue == true)
                {
                    MessageBox.Show("Заполните все параметры", "Ошибка");
                    return;
                }

                // Получаем таблицу для функции A
                var tableA = GetFunctionTable(
                    ARadioByNumber?.IsChecked == true,
                    A_n.Value ?? 1,
                    A_num.Value ?? 0,
                    A_formula?.Text
                );

                // Получаем таблицу для функции B
                var tableB = GetFunctionTable(
                    BRadioByNumber?.IsChecked == true,
                    B_n.Value ?? 1,
                    B_num.Value ?? 0,
                    B_formula?.Text
                );

                // Сравниваем
                bool areEquivalent = FunctionComparer.AreEquivalent(
                    tableA.Table, tableB.Table, out bool[] counterExample);

                if (areEquivalent)
                {
                    CompareResult.Text = "Функции ЭКВИВАЛЕНТНЫ";
                    CompareResult.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    string counterExampleStr = string.Join(" ", counterExample.Select(b => b ? "1" : "0"));
                    CompareResult.Text = $"Функции НЕ ЭКВИВАЛЕНТНЫ\nКонтрпример: {counterExampleStr}";
                    CompareResult.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сравнения: {ex.Message}", "Ошибка");
            }
        }

        private TruthTable GetFunctionTable(bool byNumber, int n, long num, string formula)
        {
            var truthTable = new TruthTable(n);

            if (byNumber)
            {
                truthTable.BuildFromNumber(num);
            }
            else
            {
                // По формуле
                if (string.IsNullOrEmpty(formula))
                    throw new Exception("Введите формулу");

                var tokens = Lexer.Lex(formula);
                var variables = tokens
                    .Where(t => t.Type == TokenType.Variable)
                    .Select(t => t.Value)
                    .Distinct()
                    .OrderBy(v => v)
                    .ToList();

                if (variables.Count != n)
                    throw new Exception($"Количество переменных в формуле ({variables.Count}) не совпадает с n={n}");

                var rpnTokens = ParserRpn.ToRpn(tokens);
                truthTable.VariableNames.Clear();
                truthTable.VariableNames.AddRange(variables);

                var allTuples = BitUtils.GenerateAllTuples(n);
                truthTable.Table.Clear();

                foreach (var tuple in allTuples)
                {
                    var variableValues = new Dictionary<string, bool>();
                    for (int i = 0; i < n; i++)
                    {
                        variableValues[variables[i]] = tuple[i];
                    }

                    bool result = ExpressionEvaluator.Evaluate(rpnTokens, variableValues);
                    truthTable.Table.Add(new TupleResult(tuple, result));
                }
            }

            return truthTable;
        }

        private void Preset_Compare_Example(object sender, RoutedEventArgs e)
        {
            // Пример: две разные функции с n=2
            A_n.Value = 2;
            A_num.Value = 3;  // 0011 - функция, равная x2
            B_n.Value = 2;
            B_num.Value = 5;  // 0101 - функция, равная x1
            CompareFunctions_Click(sender, e);
        }

        #endregion

        #region Вспомогательные методы

        private void UpdateTruthTableGrid(TruthTable truthTable)
        {
            TruthTableGrid.Columns.Clear();

            for (int i = 0; i < truthTable.VariableNames.Count; i++)
            {
                TruthTableGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = truthTable.VariableNames[i],
                    Binding = new System.Windows.Data.Binding($"Variables[{i}]") { Converter = new BoolToIntConverter() }
                });
            }

            TruthTableGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "f",
                Binding = new System.Windows.Data.Binding("Result") { Converter = new BoolToIntConverter() }
            });

            TruthTableGrid.ItemsSource = truthTable.Table;
        }

        private void UpdateFormulaTableGrid(TruthTable truthTable)
        {
            FormulaTableGrid.Columns.Clear();

            for (int i = 0; i < truthTable.VariableNames.Count; i++)
            {
                FormulaTableGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = truthTable.VariableNames[i],
                    Binding = new System.Windows.Data.Binding($"Variables[{i}]") { Converter = new BoolToIntConverter() }
                });
            }

            FormulaTableGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "f",
                Binding = new System.Windows.Data.Binding("Result") { Converter = new BoolToIntConverter() }
            });

            FormulaTableGrid.ItemsSource = truthTable.Table;
        }

        #endregion
    }

    // Конвертер для отображения bool как 0/1 в DataGrid
    public class BoolToIntConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is bool boolValue ? (boolValue ? "1" : "0") : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}