/****************************************

* 作者：闪电黑客
* 日期：2026/10/22

* 描述：运行时日志窗口。Ctrl + Alt + ~ 开关。日志文件写到 persistentDataPath。

*/

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;

namespace WorldTree.AOT
{
	/// <summary>
	/// 运行时调试窗口
	/// </summary>
	public class DebuggerGG : MonoBehaviour
	{
		/// <summary>
		/// 允许界面显示
		/// </summary>
		public bool AllowShow = true;

		/// <summary>
		/// 是否追加写到 txt
		/// </summary>
		public bool Logtxt = false;

		/// <summary>
		/// 滚动列表可显示行数
		/// </summary>
		public int LineCount = 500;

		private bool scrollLogViewBit = true;
		private float size = 1;
		private int fontSize = 14;
		private int spaceSize = 4;

		private GUIStyle btnStyle;
		private GUIStyle textStyle;

		private DebugType debugType = DebugType.Console;
		private readonly List<LogData> logDatas = new();
		private int currentLogIndex = -1;

		private readonly Dictionary<LogType, int> logsCount = new();
		private readonly Dictionary<LogType, bool> logsShow = new();
		private Vector2 scrollLogView;
		private Vector2 scrollCurrentLogView;
		private Vector2 scrollSystemView;
		private bool showMax;
		private Rect windowRect = new(0, 0, 100, 60);
		private Rect maxWindowRect = new(0, 0, 600, 450);
		private bool resizeX;
		private bool resizeY;
		private Vector2 resizeOffset;

		private int fps;
		private int frameNumber;
		private float lastShowFPSTime;
		private bool debugStop;
		private string logFilePath;

		private void Awake()
		{
			DontDestroyOnLoad(gameObject);
			logFilePath = Path.Combine(Application.persistentDataPath, "DebugLog.txt");

			foreach (LogType item in Enum.GetValues(typeof(LogType)))
			{
				logsCount.Add(item, 0);
				logsShow.Add(item, true);
			}

			Texture2D white = Texture2D.whiteTexture;
			btnStyle = new GUIStyle
			{
				alignment = TextAnchor.MiddleCenter,
			};
			btnStyle.normal.textColor = Color.black;
			btnStyle.hover.textColor = Color.black;
			btnStyle.active.textColor = Color.black;
			btnStyle.normal.background = white;
			btnStyle.hover.background = white;
			btnStyle.active.background = white;

			textStyle = new GUIStyle();
			textStyle.normal.textColor = Color.white;

			Application.logMessageReceived += LogHandler;
			Debug.Log("DebuggerGG 启动。Ctrl+Alt+~ 开关。日志文件: " + logFilePath);
		}

		private void OnDestroy()
		{
			Application.logMessageReceived -= LogHandler;
		}

		private void Update()
		{
			if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.BackQuote))
			{
				AllowShow = !AllowShow;
			}

			if (!AllowShow) return;

			frameNumber += 1;
			float time = Time.realtimeSinceStartup - lastShowFPSTime;
			if (time >= 1)
			{
				fps = (int)(frameNumber / time);
				frameNumber = 0;
				lastShowFPSTime = Time.realtimeSinceStartup;
			}
		}

		private void LogHandler(string condition, string stackTrace, LogType type)
		{
			if (logsCount.Count == 0) return;

			LogData log = new()
			{
				time = DateTime.Now.ToString("HH:mm:ss"),
				message = condition,
				stackTrace = stackTrace,
				type = type,
			};

			if (logsCount[type] < 9999) logsCount[type] = logsCount[type] + 1;
			if (!debugStop) logDatas.Add(log);
			if (logDatas.Count > LineCount) logDatas.RemoveAt(0);

			if (!Logtxt) return;
			File.AppendAllText(logFilePath, $"[{type}][{log.time}] {log.message}\r\n\r\n{log.stackTrace}\r\n");
		}

		private void OnGUI()
		{
			if (!AllowShow) return;
			if (showMax && (resizeX || resizeY))
			{
				FollowResize();
				GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none, GUIStyle.none);
			}
			if (showMax)
			{
				maxWindowRect = GUI.Window(GetInstanceID(), maxWindowRect, DrawMax, "DEBUGGER");
			}
			else
			{
				windowRect = GUI.Window(GetInstanceID(), windowRect, DrawMin, "DEBUGGER");
			}
		}

		private void DrawMin(int windowId)
		{
			GUI.DragWindow(new Rect(0, 0, 10000, 20));
			windowRect.width = 100 * size;
			windowRect.height = 60 * size;

			ColorButton(false);
			if (GUILayout.Button("FPS:" + fps, btnStyle, GUILayout.Height(30 * size)))
			{
				maxWindowRect.x = windowRect.x;
				maxWindowRect.y = windowRect.y;
				showMax = true;
			}
			ResetColor();
		}

		private void DrawMax(int windowId)
		{
			GUI.DragWindow(new Rect(0, 0, 10000, 20));
			ClampMaxSize();
			textStyle.fontSize = (int)(fontSize * size);
			btnStyle.fontSize = (int)(fontSize * size);

			GUILayout.BeginHorizontal();
			ColorButton(true);
			if (GUILayout.Button("FPS:" + fps, btnStyle, GUILayout.Width(80 * size), GUILayout.Height(30 * size)))
			{
				windowRect.x = maxWindowRect.x;
				windowRect.y = maxWindowRect.y;
				showMax = false;
			}
			ColorButton(false);
			GUILayout.Space(spaceSize);
			if (GUILayout.Button("+", btnStyle, GUILayout.Width(30 * size), GUILayout.Height(30 * size))) size += 0.5f;
			GUILayout.Space(spaceSize);
			if (GUILayout.Button("-", btnStyle, GUILayout.Width(30 * size), GUILayout.Height(30 * size)) && size > 1) size -= 0.5f;
			ResetColor();
			GUILayout.EndHorizontal();

			GUILayout.Space(spaceSize);
			GUILayout.BeginHorizontal();
			foreach (DebugType item in Enum.GetValues(typeof(DebugType)))
			{
				ColorButton(debugType == item);
				if (GUILayout.Button(item.ToString(), btnStyle, GUILayout.Width(80 * size), GUILayout.Height(30 * size)))
				{
					debugType = item;
				}
				GUILayout.Space(spaceSize);
			}
			GUILayout.EndHorizontal();
			ResetColor();

			if (debugType == DebugType.Console) DrawConsole();
			else if (debugType == DebugType.Memory) DrawMemory();
			else if (debugType == DebugType.System) DrawSystem();
			else if (debugType == DebugType.Screen) DrawScreen();
			else if (debugType == DebugType.Quality) DrawQuality();
			else DrawEnvironment();

			ResizeMaxWindow();
		}

		/// <summary>
		/// 窗口可以拉大，但不能小于 +/- 决定的基准尺寸。
		/// </summary>
		private void ClampMaxSize()
		{
			float minWidth = 600 * size;
			float minHeight = 450 * size;
			if (maxWindowRect.width < minWidth) maxWindowRect.width = minWidth;
			if (maxWindowRect.height < minHeight) maxWindowRect.height = minHeight;
		}

		private void ResizeMaxWindow()
		{
			const float grip = 14f;
			Rect right = new(maxWindowRect.width - grip, 0, grip, maxWindowRect.height);
			Rect bottom = new(0, maxWindowRect.height - grip, maxWindowRect.width, grip);
			ColorButton(false);
			GUI.Box(new Rect(maxWindowRect.width - grip, maxWindowRect.height - grip, grip, grip), "");
			ResetColor();

			Event current = Event.current;
			if (current.type != EventType.MouseDown) return;
			if (!right.Contains(current.mousePosition) && !bottom.Contains(current.mousePosition)) return;

			resizeX = right.Contains(current.mousePosition);
			resizeY = bottom.Contains(current.mousePosition);
			Vector2 screen = GUIUtility.GUIToScreenPoint(current.mousePosition);
			resizeOffset = new Vector2(screen.x - maxWindowRect.xMax, screen.y - maxWindowRect.yMax);
			current.Use();
		}

		/// <summary>
		/// 用屏幕坐标把边对齐到鼠标，避免窗口坐标系跟着变大以后增量越拖越偏。
		/// </summary>
		private void FollowResize()
		{
			if (!Input.GetMouseButton(0))
			{
				resizeX = false;
				resizeY = false;
				return;
			}

			Vector2 mouse = Input.mousePosition;
			mouse.y = Screen.height - mouse.y;
			float minWidth = 600 * size;
			float minHeight = 450 * size;
			if (resizeX) maxWindowRect.width = Mathf.Max(minWidth, mouse.x - maxWindowRect.x - resizeOffset.x);
			if (resizeY) maxWindowRect.height = Mathf.Max(minHeight, mouse.y - maxWindowRect.y - resizeOffset.y);
		}

		private void DrawConsole()
		{
			GUILayout.BeginHorizontal();
			ColorButton(false);
			if (GUILayout.Button("Clear", btnStyle, GUILayout.Width(80 * size), GUILayout.Height(30 * size)))
			{
				logDatas.Clear();
				foreach (LogType item in Enum.GetValues(typeof(LogType))) logsCount[item] = 0;
				currentLogIndex = -1;
			}
			GUILayout.Space(spaceSize);
			ColorButton(debugStop);
			if (GUILayout.Button("Stop", btnStyle, GUILayout.Width(80 * size), GUILayout.Height(30 * size))) debugStop = !debugStop;
			GUILayout.Space(spaceSize);
			ColorButton(scrollLogViewBit);
			if (GUILayout.Button("ScrollLog", btnStyle, GUILayout.Width(80 * size), GUILayout.Height(30 * size))) scrollLogViewBit = !scrollLogViewBit;
			GUILayout.Space(spaceSize);
			ColorButton(Logtxt);
			if (GUILayout.Button("Logtxt", btnStyle, GUILayout.Width(80 * size), GUILayout.Height(30 * size))) Logtxt = !Logtxt;
			ResetColor();
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			foreach (LogType item in Enum.GetValues(typeof(LogType)))
			{
				ColorButton(logsShow[item]);
				if (GUILayout.Button(item + " [" + logsCount[item] + "]", btnStyle, GUILayout.Width(110 * size), GUILayout.Height(30 * size)))
				{
					logsShow[item] = !logsShow[item];
				}
				GUILayout.Space(spaceSize);
			}
			GUILayout.EndHorizontal();
			ResetColor();

			scrollLogView = GUILayout.BeginScrollView(scrollLogView, "Box", GUILayout.Height(165 * size));
			for (int i = 0; i < logDatas.Count; i++)
			{
				if (!logsShow[logDatas[i].type]) continue;
				GUILayout.BeginHorizontal();
				ColorButton(currentLogIndex == i);
				if (GUILayout.Button("x", btnStyle, GUILayout.Width(20 * size), GUILayout.Height(20 * size))) currentLogIndex = i;
				ResetColor();
				GUI.contentColor = LogColor(logDatas[i].type);
				GUILayout.Label($" [{logDatas[i].type}] [{logDatas[i].time}] {logDatas[i].message}", textStyle);
				GUI.contentColor = Color.white;
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();
			if (scrollLogViewBit) scrollLogView.y += size;

			bool hasLog = currentLogIndex >= 0 && currentLogIndex < logDatas.Count;
			ColorButton(hasLog);
			if (GUILayout.Button("复制", btnStyle, GUILayout.Width(80 * size), GUILayout.Height(30 * size)) && hasLog)
			{
				LogData log = logDatas[currentLogIndex];
				GUIUtility.systemCopyBuffer = $"[{log.type}] [{log.time}] {log.message}\r\n\r\n{log.stackTrace}";
			}
			ResetColor();

			scrollCurrentLogView = GUILayout.BeginScrollView(scrollCurrentLogView, "Box");
			if (hasLog)
			{
				GUILayout.Label(logDatas[currentLogIndex].message + "\r\n\r\n" + logDatas[currentLogIndex].stackTrace, textStyle);
			}
			GUILayout.EndScrollView();
		}

		private void DrawMemory()
		{
			GUILayout.Label("Memory Information", textStyle);
			GUILayout.BeginVertical("Box");
			GUILayout.Label("总内存：" + Profiler.GetTotalReservedMemoryLong() / 1000000 + "MB", textStyle);
			GUILayout.Label("已占用内存：" + Profiler.GetTotalAllocatedMemoryLong() / 1000000 + "MB", textStyle);
			GUILayout.Label("空闲中内存：" + Profiler.GetTotalUnusedReservedMemoryLong() / 1000000 + "MB", textStyle);
			GUILayout.Label("总Mono堆内存：" + Profiler.GetMonoHeapSizeLong() / 1000000 + "MB", textStyle);
			GUILayout.Label("已占用Mono堆内存：" + Profiler.GetMonoUsedSizeLong() / 1000000 + "MB", textStyle);
			GUILayout.EndVertical();
			ColorButton(false);
			if (GUILayout.Button("卸载未使用的资源", btnStyle)) Resources.UnloadUnusedAssets();
			if (GUILayout.Button("使用GC垃圾回收", btnStyle)) GC.Collect();
			ResetColor();
		}

		private void DrawSystem()
		{
			GUILayout.Label("System Information", textStyle);
			scrollSystemView = GUILayout.BeginScrollView(scrollSystemView, "Box");
			GUILayout.Label("操作系统：" + SystemInfo.operatingSystem, textStyle);
			GUILayout.Label("系统内存：" + SystemInfo.systemMemorySize + "MB", textStyle);
			GUILayout.Label("处理器：" + SystemInfo.processorType, textStyle);
			GUILayout.Label("处理器数量：" + SystemInfo.processorCount, textStyle);
			GUILayout.Label("显卡：" + SystemInfo.graphicsDeviceName, textStyle);
			GUILayout.Label("显卡类型：" + SystemInfo.graphicsDeviceType, textStyle);
			GUILayout.Label("显存：" + SystemInfo.graphicsMemorySize + "MB", textStyle);
			GUILayout.Label("显卡标识：" + SystemInfo.graphicsDeviceID, textStyle);
			GUILayout.Label("显卡供应商：" + SystemInfo.graphicsDeviceVendor, textStyle);
			GUILayout.Label("显卡供应商标识码：" + SystemInfo.graphicsDeviceVendorID, textStyle);
			GUILayout.Label("设备模式：" + SystemInfo.deviceModel, textStyle);
			GUILayout.Label("设备名称：" + SystemInfo.deviceName, textStyle);
			GUILayout.Label("设备类型：" + SystemInfo.deviceType, textStyle);
			GUILayout.Label("设备标识：" + SystemInfo.deviceUniqueIdentifier, textStyle);
			GUILayout.EndScrollView();
		}

		private void DrawScreen()
		{
			GUILayout.Label("Screen Information", textStyle);
			GUILayout.BeginVertical("Box");
			GUILayout.Label("DPI：" + Screen.dpi, textStyle);
			GUILayout.Label("分辨率：" + Screen.currentResolution, textStyle);
			GUILayout.EndVertical();
			ColorButton(Screen.fullScreen);
			if (GUILayout.Button("全屏", btnStyle))
			{
				Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, !Screen.fullScreen);
			}
			ResetColor();
		}

		private void DrawQuality()
		{
			GUILayout.Label("Quality Information", textStyle);
			string value = "";
			if (QualitySettings.GetQualityLevel() == 0) value = " [最低]";
			else if (QualitySettings.GetQualityLevel() == QualitySettings.names.Length - 1) value = " [最高]";
			GUILayout.Label("图形质量：" + QualitySettings.names[QualitySettings.GetQualityLevel()] + value, textStyle);
			ColorButton(false);
			if (GUILayout.Button("降低一级图形质量", btnStyle)) QualitySettings.DecreaseLevel();
			if (GUILayout.Button("提升一级图形质量", btnStyle)) QualitySettings.IncreaseLevel();
			ResetColor();
		}

		private void DrawEnvironment()
		{
			GUILayout.Label("Environment Information", textStyle);
			GUILayout.BeginVertical("Box");
			GUILayout.Label("项目名称：" + Application.productName, textStyle);
			GUILayout.Label("项目ID：" + Application.identifier, textStyle);
			GUILayout.Label("项目版本：" + Application.version, textStyle);
			GUILayout.Label("Unity版本：" + Application.unityVersion, textStyle);
			GUILayout.Label("公司名称：" + Application.companyName, textStyle);
			GUILayout.Label("日志文件：" + logFilePath, textStyle);
			GUILayout.EndVertical();
			ColorButton(false);
			if (GUILayout.Button("退出程序", btnStyle)) Application.Quit();
			ResetColor();
		}

		/// <summary>
		/// 选中绿色，未选中灰色，字固定黑色。
		/// </summary>
		private static void ColorButton(bool selected)
		{
			GUI.backgroundColor = selected ? Color.green : Color.gray;
			GUI.contentColor = Color.black;
		}

		private static void ResetColor()
		{
			GUI.backgroundColor = Color.white;
			GUI.contentColor = Color.white;
		}

		private static Color LogColor(LogType type) => type switch
		{
			LogType.Error or LogType.Exception => Color.red,
			LogType.Warning => Color.yellow,
			LogType.Assert => Color.cyan,
			_ => Color.white,
		};

		private struct LogData
		{
			public string time;
			public LogType type;
			public string message;
			public string stackTrace;
		}

		private enum DebugType
		{
			Console,
			Memory,
			System,
			Screen,
			Quality,
			Environment,
		}
	}
}
