// 如果 using System.Windows.Automation; 提示不存在，请确保：
// 1. 项目目标框架为 net6.0‑windows（或更高版本）或 .NET Framework 4.7.2+；
// 2. 安装了 NuGet 包：Microsoft.Windows.Compatibility 或 System.Windows.Automation；
// 3. 或者手动添加对 UIAutomationClient.dll 和 UIAutomationTypes.dll 的引用。
// 满足以上条件后，本文件即可正常编译。

using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Automation;

namespace UIAgentLib
{
    /// <summary>
    /// 提供 Windows UI Automation 常用操作的静态方法。
    /// 使用前请确保项目引用了 UIAutomationClient.dll 和 UIAutomationTypes.dll。
    /// </summary>
    public static class UIAgent
    {
        // ========== 基础获取 ==========

        /// <summary> 获得桌面根元素（RootElement） </summary>
        public static AutomationElement GetDesktopRoot() => AutomationElement.RootElement;

        /// <summary> 获得当前具有键盘焦点的元素 </summary>
        public static AutomationElement GetFocusedElement() => AutomationElement.FocusedElement;

        /// <summary> 根据屏幕坐标获取最顶层的 UI 元素 </summary>
        public static AutomationElement GetElementFromPoint(int x, int y)
            => AutomationElement.FromPoint(new System.Windows.Point(x, y));

        /// <summary> 获得所有顶级窗口 </summary>
        public static AutomationElementCollection GetTopLevelWindows()
            => AutomationElement.RootElement.FindAll(
                TreeScope.Children,
                Condition.TrueCondition);

        /// <summary> 获取元素所属进程的友好名称 </summary>
        public static string GetProcessName(AutomationElement element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            int pid = element.Current.ProcessId;
            try
            {
                using var process = Process.GetProcessById(pid);
                return process.ProcessName;
            }
            catch
            {
                return null;
            }
        }

        // ========== 查找 ==========

        /// <summary> 在某个元素子树中按 AutomationId 查找第一个匹配项 </summary>
        public static AutomationElement FindByAutomationId(
            AutomationElement parent,
            string automationId)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            return parent.FindFirst(
                TreeScope.Descendants,
                new PropertyCondition(
                    AutomationElement.AutomationIdProperty, automationId));
        }

        /// <summary> 在某个元素子树中按 Name 属性查找第一个匹配项 </summary>
        public static AutomationElement FindByName(
            AutomationElement parent,
            string name)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            return parent.FindFirst(
                TreeScope.Descendants,
                new PropertyCondition(
                    AutomationElement.NameProperty, name));
        }

        // ========== 等待元素出现（简单轮询） ==========

        /// <summary> 等待子树中出现符合 AutomationId 的元素，超时返回 null </summary>
        public static AutomationElement WaitForElementByAutomationId(
            AutomationElement parent,
            string automationId,
            int timeoutMilliseconds = 5000)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            var deadline = Environment.TickCount + timeoutMilliseconds;
            while (Environment.TickCount < deadline)
            {
                var found = parent.FindFirst(
                    TreeScope.Descendants,
                    new PropertyCondition(
                        AutomationElement.AutomationIdProperty, automationId));
                if (found != null)
                    return found;

                Thread.Sleep(250);
            }
            return null;
        }

        /// <summary> 等待子树中出现符合 Name 的元素，超时返回 null </summary>
        public static AutomationElement WaitForElementByName(
            AutomationElement parent,
            string name,
            int timeoutMilliseconds = 5000)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            var deadline = Environment.TickCount + timeoutMilliseconds;
            while (Environment.TickCount < deadline)
            {
                var found = parent.FindFirst(
                    TreeScope.Descendants,
                    new PropertyCondition(
                        AutomationElement.NameProperty, name));
                if (found != null)
                    return found;

                Thread.Sleep(250);
            }
            return null;
        }

        // ========== 模式操作 ==========

        /// <summary> 尝试对元素执行 Invoke（如点击按钮） </summary>
        public static void Click(AutomationElement element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (element.TryGetCurrentPattern(InvokePattern.Pattern, out object patternObj))
            {
                ((InvokePattern)patternObj).Invoke();
            }
            else
            {
                throw new InvalidOperationException(
                    "目标元素不支持 InvokePattern，无法点击。");
            }
        }

        /// <summary> 尝试为元素设置 Value（输入文本） </summary>
        public static void SetValue(AutomationElement element, string value)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object patternObj))
            {
                ((ValuePattern)patternObj).SetValue(value);
            }
            else
            {
                // 后备方法：尝试通过 LegacyIAccessible 或发送按键，此处仅抛出异常
                throw new InvalidOperationException(
                    "目标元素不支持 ValuePattern，无法设置文本。");
            }
        }

        /// <summary> 尝试获取元素的 Value </summary>
        public static string GetValue(AutomationElement element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object patternObj))
            {
                return ((ValuePattern)patternObj).Current.Value;
            }
            return string.Empty;
        }

        /// <summary> 尝试获取元素的 Name 属性 </summary>
        public static string GetName(AutomationElement element)
            => element?.Current.Name;

        /// <summary> 尝试获取元素的 AutomationId </summary>
        public static string GetAutomationId(AutomationElement element)
            => element?.Current.AutomationId;

        /// <summary> 尝试获取元素的 ControlType 本地化名称 </summary>
        public static string GetControlType(AutomationElement element)
            => element?.Current.LocalizedControlType;
    }
}
