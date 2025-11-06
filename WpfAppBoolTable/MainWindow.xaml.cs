using ClassLibraryBoolTable.Service;
using ClassLibraryBoolTable.Util;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfAppBoolTable
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        public class TruthRow
        {
            public bool X1 { get; set; }
            public bool X2 { get; set; }
            public bool X3 { get; set; }
            public bool X4 { get; set; }
            public bool X5 { get; set; }
            public bool F { get; set; }
        }

        private void BuildTruthTable(object sender, RoutedEventArgs e)
        {
            int n = InputN.Value ?? 0;
            long num = InputNum.Value ?? 0;

            // создаём таблицу истинности с помощью LogicLibrary
            var logicTable = new TruthTable(n);
            logicTable.BuildFromNumber(num);

            // создаём DataTable для DataGrid
            DataTable dt = new DataTable();

            // Добавляем колонки x1, x2, ..., xn
            for (int i = 1; i <= n; i++)
                dt.Columns.Add($"x{i}", typeof(int));

            // Добавляем колонку результата f
            dt.Columns.Add("f", typeof(int));

            // Заполняем строки
            foreach (var tuple in logicTable.Table)
            {
                var row = dt.NewRow();
                for (int i = 0; i < n; i++)
                    row[$"x{i + 1}"] = tuple.Variables[i] ? 1 : 0;
                row["f"] = tuple.Result ? 1 : 0;
                dt.Rows.Add(row);
            }

            // Привязываем DataTable к DataGrid
            TruthTableGrid.ItemsSource = dt.DefaultView;

            var bits = BitUtils.DecodeFunction(n, num);
            string binaryString = string.Join("", bits.Select(b => b ? "1" : "0"));
            BinaryExplanation.Text = $"f({n}) = {binaryString}₂ (от младшего к старшему биту)";


            // Очищаем старые DNF/KNF
            OutputDNF.Clear();
            OutputKNF.Clear();
        }



        private void BuildDNF(object sender, RoutedEventArgs e)
        {
            int n = InputN.Value ?? 0;
            long num = InputNum.Value ?? 0;

            var table = new TruthTable(n);
            table.BuildFromNumber(num);

            OutputDNF.Text = DnfGenerator.BuildDNF(table.Table, table.VariableNames);
        }

        private void BuildKNF(object sender, RoutedEventArgs e)
        {
            int n = InputN.Value ?? 0;
            long num = InputNum.Value ?? 0;

            var table = new TruthTable(n);
            table.BuildFromNumber(num);

            OutputKNF.Text = KnfGenerator.BuildKNF(table.Table, table.VariableNames);
        }

    }
}