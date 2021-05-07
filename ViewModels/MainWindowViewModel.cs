using KantorLr7.Infrastructure.Commands;
using KantorLr7.Model.Data;
using KantorLr7.ViewModels.Base;
using CompMathLibrary.Interpolation.Splines;
using org.mariuszgromada.math.mxparser;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Input;
using System.Windows.Markup;

namespace KantorLr7.ViewModels
{
	[MarkupExtensionReturnType(typeof(MainWindowViewModel))]
	public class MainWindowViewModel : ViewModel
	{
		private const string SPLINE_FILE = "spline.dat";
		private InterpolatingCubicSpline _spline;
		public MainWindowViewModel()
		{
			CalculateFunctionValueInPointCommand = new LambdaCommand(OnCalculateFunctionValueInPointCommandExecuted, CanCalculateFunctionValueInPointCommandExecute);
			RestoreSplineTableCommand = new LambdaCommand(OnRestoreSplineTableCommandExecuted, CanRestoreLagrangeTableCommandExecute);
			SaveSplineTableCommand = new LambdaCommand(OnSaveSplineTableCommandExecuted, CanSaveSplineTableCommandCommandExecute);
			
			
			
			AddNewPointCommand = new LambdaCommand(OnAddNewPointCommandExecuted, CanAddNewPointCommandExecute);
			RemoveSelectedPointCommand = new LambdaCommand(OnRemoveSelectedPointCommandExecuted, CanRemoveSelectedPointCommandExecute);
			BuildFunctionGraphicCommand = new LambdaCommand(OnBuildFunctionGraphicCommandExecuted, CanBuildFunctionGraphicCommandExecute);
			BuildSplineGraphicCommand = new LambdaCommand(OnBuildSplineGraphicCommandExecuted, CanBuildSplineGraphicCommandExecute);
			GenerateTableCommand = new LambdaCommand(OnGenerateTableCommandExecuted, CanGenerateTableCommandExecute);
		}

		#region Properties
		private string _title = "Title";
		public string Title { get => _title; set => Set(ref _title, value); }

		private string _status = "Интерполирование многочленами";
		public string Status { get => _status; set => Set(ref _status, value); }

		public ObservableCollection<Point> PointsFunction { get; set; } = new ObservableCollection<Point>();
		public ObservableCollection<Point> PointsSpline { get; set; } = new ObservableCollection<Point>();

		private string _functionText = "";
		public string FunctionText
		{
			get => _functionText;
			set
			{
				Set(ref _functionText, value);
				GraphTitle = value;
			}
		}

		private string _graphTitle = "График функции -- и соответствующего сплайна";
		public string GraphTitle
		{
			get => _graphTitle;
			set
			{
				if (value is null || string.IsNullOrWhiteSpace(value))
					Set(ref _graphTitle, "График функции -- и соответствующего сплайна");
				else
					Set(ref _graphTitle, $"График функции {value} и соответствующего сплайна");
			}
		}

		private string _pointOfCalculation;
		public string PointOfCalculation { get => _pointOfCalculation; set => Set(ref _pointOfCalculation, value); }

		private double _functionValueInPoint = double.NaN;
		public double FunctionValueInPoint { get => _functionValueInPoint; set => Set(ref _functionValueInPoint, value); }

		private string _argumentsArray;
		public string ArgumentsArray { get => _argumentsArray; set => Set(ref _argumentsArray, value); }

		public ObservableCollection<Point> SplineTable { get; set; } = new ObservableCollection<Point>();
		

		private Point _selectedPoint;
		public Point SelectedPoint { get => _selectedPoint; set => Set(ref _selectedPoint, value); }

		private string _argumentLeftBoard;
		public string ArgumentLeftBoard { get => _argumentLeftBoard; set => Set(ref _argumentLeftBoard, value); }

		private string _argumentRightBoard;
		public string ArgumentRightBoard { get => _argumentRightBoard; set => Set(ref _argumentRightBoard, value); }

		private string _step;
		public string Step { get => _step; set => Set(ref _step, value); }

		private string _generateTableLeftBoard;
		public string GenerateTableLeftBoard { get => _generateTableLeftBoard; set => Set(ref _generateTableLeftBoard, value); }

		private string _generateTableRightBoard;
		public string GenerateTableRightBoard { get => _generateTableRightBoard; set => Set(ref _generateTableRightBoard, value); }

		private string _generateTableStep;
		public string GenerateTableStep { get => _generateTableStep; set => Set(ref _generateTableStep, value); }

		private string _maxDeviationValue;
		public string MaxDeviationValue { get => _maxDeviationValue; set => Set(ref _maxDeviationValue, value); }

		private string _maxDeviationPoint;
		public string MaxDeviationPoint { get => _maxDeviationPoint; set => Set(ref _maxDeviationPoint, value); }
		#endregion

		#region Commands

		public ICommand CalculateFunctionValueInPointCommand { get; }
		private void OnCalculateFunctionValueInPointCommandExecuted(object p)
		{
			try
			{
				FunctionValueInPoint = double.NaN;
				double point = Convert.ToDouble(PointOfCalculation);
				double[] args = new double[SplineTable.Count];
				double[] values = new double[SplineTable.Count];
				for (int i = 0; i < SplineTable.Count; i++)
				{
					args[i] = SplineTable[i].X;
					values[i] = SplineTable[i].Y;
				}
				_spline = new InterpolatingCubicSpline(args, values);
				FunctionValueInPoint = _spline.GetFunctionValueInPoint(point);
				Status = "Успешное выполнение";
			}
			catch (Exception e)
			{
				Status = $"Опреация провалена. Причина: {e.Message}";
			}
		}
		private bool CanCalculateFunctionValueInPointCommandExecute(object p)
		{
			return !(SplineTable.Count == 0 || string.IsNullOrWhiteSpace(PointOfCalculation));
		}

		public ICommand RestoreSplineTableCommand { get; }
		private void OnRestoreSplineTableCommandExecuted(object p)
		{
			try
			{
				SplineTable.Clear();
				StreamReader reader = new StreamReader(SPLINE_FILE);
				string[] points = reader.ReadToEnd().Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
				string[] coords;
				for (int i = 0; i < points.Length; i++)
				{
					coords = points[i].Split(" ", StringSplitOptions.RemoveEmptyEntries);
					SplineTable.Add(new Point(Convert.ToDouble(coords[0]), Convert.ToDouble(coords[1])));
				}
				reader.Close();
				Status = "Данные восстановлены";
			}
			catch (Exception e)
			{
				Status = $"Опреация провалена. Причина: {e.Message}";
			}
		}
		private bool CanRestoreLagrangeTableCommandExecute(object p)
		{
			return File.Exists(SPLINE_FILE);
		}

		public ICommand SaveSplineTableCommand { get; }
		private void OnSaveSplineTableCommandExecuted(object p)
		{
			try
			{
				StreamWriter writer = new StreamWriter(SPLINE_FILE);
				for (int i = 0; i < SplineTable.Count; i++)
				{
					writer.WriteLine($"{SplineTable[i].X} {SplineTable[i].Y}");
				}
				writer.Close();
				Status = $"Данные записаны в файл {SPLINE_FILE}";
			}
			catch (Exception e)
			{
				Status = $"Опреация провалена. Причина: {e.Message}";
			}
		}
		private bool CanSaveSplineTableCommandCommandExecute(object p)
		{
			return SplineTable.Count > 0;
		}

		public ICommand AddNewPointCommand { get; }
		private void OnAddNewPointCommandExecuted(object p)
		{
			try
			{

				SplineTable.Add(new Point(0, 0));
				Status = $"Строка добавлена";
			}
			catch (Exception e)
			{
				Status = $"Опреация провалена. Причина: {e.Message}";
			}
		}
		private bool CanAddNewPointCommandExecute(object p)
		{
			return true;
		}

		public ICommand RemoveSelectedPointCommand { get; }
		private void OnRemoveSelectedPointCommandExecuted(object p)
		{
			try
			{
				SplineTable.Remove(SelectedPoint);
				SelectedPoint = null;
				Status = $"Строка удалена";
			}
			catch (Exception e)
			{
				Status = $"Опреация провалена. Причина: {e.Message}";
			}
		}
		private bool CanRemoveSelectedPointCommandExecute(object p)
		{
			return SelectedPoint != null;
		}

		public ICommand BuildFunctionGraphicCommand { get; }
		private void OnBuildFunctionGraphicCommandExecuted(object p)
		{
			try
			{
				Function f = new Function(FunctionText);
				Expression expression;
				PointsFunction.Clear();
				double left = Convert.ToDouble(ArgumentLeftBoard);
				double right = Convert.ToDouble(ArgumentRightBoard);
				double step = Convert.ToDouble(Step);
				Status = "Строю...";
				for (double i = left; i <= right; i += step)
				{
					expression = new Expression($"f({i.ToString().Replace(",", ".")})", f);
					PointsFunction.Add(new Point(i, expression.calculate()));
				}
				FindMaxDeviation();
				Status = "График функции построен";
			}
			catch (Exception e)
			{
				Status = $"Опреация провалена. Причина: {e.Message}";
			}
		}
		private bool CanBuildFunctionGraphicCommandExecute(object p)
		{
			return !(string.IsNullOrWhiteSpace(ArgumentLeftBoard) || string.IsNullOrWhiteSpace(ArgumentRightBoard) || string.IsNullOrWhiteSpace(Step) || string.IsNullOrWhiteSpace(FunctionText));
		}

		public ICommand BuildSplineGraphicCommand { get; }
		private void OnBuildSplineGraphicCommandExecuted(object p)
		{
			try
			{
				PointsSpline.Clear();
				Status = "Строю...";
				double[] args;
				double[] values;
				double left = Convert.ToDouble(ArgumentLeftBoard);
				double right = Convert.ToDouble(ArgumentRightBoard);
				double step = Convert.ToDouble(Step);
				if (SplineTable.Count > 0)
				{
					args = new double[SplineTable.Count];
					values = new double[SplineTable.Count];
					for (int i = 0; i < args.Length; i++)
					{
						args[i] = SplineTable[i].X;
						values[i] = SplineTable[i].Y;
					}
					_spline = new InterpolatingCubicSpline(args, values);
					List<double> otherArgs = new List<double>();
					for (double i = left; i <= right; i += step)
					{
						PointsSpline.Add(new Point(i, _spline.GetFunctionValueInPoint(i)));
					}
					FindMaxDeviation();
					Status = "График многочлена построен";
				}
				else
					Status = "График длжна быть построена таблица";
			}
			catch (Exception e)
			{
				Status = $"Опреация провалена. Причина: {e.Message}";
			}
		}
		private bool CanBuildSplineGraphicCommandExecute(object p)
		{
			return SplineTable.Count > 0 && CanBuildFunctionGraphicCommandExecute(p);
		}

		public ICommand GenerateTableCommand { get; }
		private void OnGenerateTableCommandExecuted(object p)
		{
			try
			{
				double left = Convert.ToDouble(GenerateTableLeftBoard);
				double right = Convert.ToDouble(GenerateTableRightBoard);
				double step = Convert.ToDouble(GenerateTableStep);
				SplineTable.Clear();
				SelectedPoint = null;
				Function f = new Function(FunctionText);
				Expression expression;
				for (double i = left; i <= right; i += step)
				{
					expression = new Expression($"f({i.ToString().Replace(",", ".")})", f);
					SplineTable.Add(new Point(i, expression.calculate()));
				}
				Status = $"Таблица сгенерирована";
			}
			catch (Exception e)
			{
				Status = $"Опреация провалена. Причина: {e.Message}";
			}
		}
		private bool CanGenerateTableCommandExecute(object p)
		{
			return !(string.IsNullOrWhiteSpace(GenerateTableLeftBoard) || string.IsNullOrWhiteSpace(GenerateTableRightBoard) || string.IsNullOrWhiteSpace(GenerateTableStep) || string.IsNullOrWhiteSpace(FunctionText));
		}
		#endregion

		private void FindMaxDeviation()
		{
			MaxDeviationPoint = "";
			MaxDeviationValue = "";
			if (PointsFunction.Count != PointsSpline.Count)
				return;
			double maxPoint = PointsFunction[0].X;
			double maxValue = Math.Abs(PointsFunction[0].Y - PointsSpline[0].Y);
			double currentMaxValue;
			for (int i = 1; i < PointsFunction.Count; i++)
			{
				currentMaxValue = Math.Abs(PointsFunction[i].Y - PointsSpline[i].Y);
				if (currentMaxValue > maxValue)
				{
					maxValue = currentMaxValue;
					maxPoint = PointsFunction[i].X;
				}
			}
			MaxDeviationPoint = maxPoint.ToString();
			MaxDeviationValue = maxValue.ToString();
		}
	}
}
