﻿using System;
using System.Globalization;
using System.Windows.Forms;
using System.Management;
using System.ComponentModel;
using System.Reflection;
using WMIViewer.Properties;

namespace WMIViewer
{
    public sealed partial class WmiForm : Form
    {
        private readonly WmiPresenter m_Presenter;
        private readonly WmiArgs m_Args;
        private string m_CurrentWmiClass;

        public event EventHandler FocusedItemChanged;

        [Localizable(false)]
        public WmiForm(WmiPresenter presenter)
        {
            if (presenter == null)
                throw new ArgumentNullException("presenter");
            m_Presenter = presenter;
            m_Presenter.View = this;
            m_Args = m_Presenter.Args;
            InitializeComponent();
            // Enable double buffer for ListView
            var mi = typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic);
            mi.Invoke(lvInstances, new object[] { ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true });

            FocusedItemChanged += lvInstances_FocusedItemChanged;
            UpdateTitle();
            ShowStat(WmiClassList.Instance.ClassCount, WmiClassList.Instance.PropCount, WmiClassList.Instance.MethodCount);
            Icon = Resources.WMIViewer16;
        }

        public void UpdateTitle()
        {
            var description = WmiClassList.GetPropertyValue(m_Presenter.Namespace, "Win32_OperatingSystem",
                "Description");
            if (string.IsNullOrEmpty(description))
                Text = Resources.WMIForm_CompPrefix + m_Args.ComputerName;
            else
                Text = string.Format(CultureInfo.InvariantCulture, Resources.WMIForm_Title, m_Args.ComputerName, description);
        }

        public WmiPresenter Presenter
        {
            get { return m_Presenter; }
        }

        [Localizable(false)]
        private void lvInstances_FocusedItemChanged(object sender, EventArgs e)
        {
            if (m_Presenter.WmiClass == null) return;
            if (lvInstances.FocusedItem == null) return;
            m_Presenter.WmiObject = (ManagementObject)lvInstances.FocusedItem.Tag;
            if (m_Presenter.WmiObject == null) return;

            var dynObj = new DynamicObject();
            foreach (var prop in m_Presenter.WmiObject.Properties)
            {
                // skip array of bytes
                if (prop.Type == CimType.UInt8 && prop.IsArray)
                    continue;

                var classProp = m_Presenter.WmiClass.Properties[prop.Name];

                bool isCimKey = false;
                bool isReadOnly = true;
                string description = "";

                foreach (QualifierData qd in classProp.Qualifiers)
                {
                    if (qd.Name.Equals("CIM_Key"))
                        isCimKey = true;
                    if (qd.Name.Equals("write"))
                        isReadOnly = false;
                    if (qd.Name.Equals("Description"))
                        description = qd.Value.ToString();
                }
                if (isCimKey) continue;
                string category = prop.Type.ToString();
                switch (prop.Type)
                {

                    //     A signed 16-bit integer. This value maps to the System.Int16 type.
                    case CimType.SInt16:
                        DynObjAddProperty<Int16>(dynObj, prop, description, category, isReadOnly);
                        break;
                    //     A signed 32-bit integer. This value maps to the System.Int32 type.
                    case CimType.SInt32:
                        DynObjAddProperty<Int32>(dynObj, prop, description, category, isReadOnly);
                        break;
                    //     A floating-point 32-bit number. This value maps to the System.Single type.
                    case CimType.Real32:
                        DynObjAddProperty<Single>(dynObj, prop, description, category, isReadOnly);
                        break;
                    //     A floating point 64-bit number. This value maps to the System.Double type.
                    case CimType.Real64:
                        DynObjAddProperty<Double>(dynObj, prop, description, category, isReadOnly);
                        break;
                    //     A string. This value maps to the System.String type.
                    case CimType.String:
                        DynObjAddProperty<String>(dynObj, prop, description, category, isReadOnly);
                        break;
                    //     A Boolean. This value maps to the System.Boolean type.
                    case CimType.Boolean:
                        DynObjAddProperty<Boolean>(dynObj, prop, description, category, isReadOnly);
                        break;
                    //     An embedded object. Note that embedded objects differ from references in
                    //     that the embedded object does not have a path and its lifetime is identical
                    //     to the lifetime of the containing object. This value maps to the System.Object
                    //     type.
                    case CimType.Object:
                        DynObjAddProperty<Object>(dynObj, prop, description, category, isReadOnly);
                        break;
                    //     A signed 8-bit integer. This value maps to the System.SByte type.
                    case CimType.SInt8:
                        DynObjAddProperty<SByte>(dynObj, prop, description, category, isReadOnly);
                        break;
                    //     An unsigned 8-bit integer. This value maps to the System.Byte type.
                    case CimType.UInt8:
                        DynObjAddProperty<Byte>(dynObj, prop, description, category, isReadOnly);
                        break;
                    //     An unsigned 16-bit integer. This value maps to the System.UInt16 type.
                    case CimType.UInt16:
                        DynObjAddProperty<UInt16>(dynObj, prop, description, category, isReadOnly);
                        break;
                    //     An unsigned 32-bit integer. This value maps to the System.UInt32 type.
                    case CimType.UInt32:
                        DynObjAddProperty<UInt32>(dynObj, prop, description, category, isReadOnly);
                        break;
                    //     A signed 64-bit integer. This value maps to the System.Int64 type.
                    case CimType.SInt64:
                        DynObjAddProperty<Int64>(dynObj, prop, description, category, isReadOnly);
                        break;
                    //     An unsigned 64-bit integer. This value maps to the System.UInt64 type.
                    case CimType.UInt64:
                        DynObjAddProperty<UInt64>(dynObj, prop, description, category, isReadOnly);
                        break;
                    //     A date or time value, represented in a string in DMTF date/time format: yyyymmddHHMMSS.mmmmmmsUUU,
                    //     where yyyymmdd is the date in year/month/day; HHMMSS is the time in hours/minutes/seconds;
                    //     mmmmmm is the number of microseconds in 6 digits; and sUUU is a sign (+ or
                    //     -) and a 3-digit UTC offset. This value maps to the System.DateTime type.
                    case CimType.DateTime:
                        if (prop.Value == null)
                            dynObj.AddPropertyNull<DateTime>(prop.Name, prop.Name, description, category, isReadOnly, null);
                        else
                            dynObj.AddProperty(prop.Name, WmiHelper.ToDateTime(prop.Value.ToString()), description, category, isReadOnly);
                        break;
                    //     A reference to another object. This is represented by a string containing
                    //     the path to the referenced object. This value maps to the System.Int16 type.
                    case CimType.Reference:
                        DynObjAddProperty<Int16>(dynObj, prop, description, category, isReadOnly);
                        break;
                    //     A 16-bit character. This value maps to the System.Char type.
                    case CimType.Char16:
                        DynObjAddProperty<Char>(dynObj, prop, description, category, isReadOnly);
                        break;
                    default:
                        string value = prop.Value == null ? null : prop.Value.ToString();
                        dynObj.AddProperty(String.Format(CultureInfo.InvariantCulture, "{0} : {1}", prop.Name, prop.Type), value, description, "Unknown", isReadOnly);
                        break;
                }
            }
            PropGrid.SelectedObject = dynObj;
        }

        public ListView LV
        {
            get { return lvInstances; }
        }

        public string CurrentWmiClass
        {
            get
            {
                return m_CurrentWmiClass;
            }
            set
            {
                m_CurrentWmiClass = value;
                lDescription.Text = WmiClassList.Instance.GetClassDescription(m_Presenter.Namespace, value);
                lClassName.Text = value;
                m_Presenter.EnumObjects(value);
                m_Presenter.BuildContextMenu(menuCommands.Items);
                //m_Presenter.BuildContextMenu(mMethod.DropDownItems);
                if (lvInstances.Items.Count == 0)
                    PropGrid.SelectedObject = null;
                else
                {
                    lvInstances.FocusedItem = lvInstances.Items[0];
                    lvInstances.FocusedItem.Selected = true;
                    lvInstances_FocusedItemChanged(lvInstances, EventArgs.Empty);
                    lvInstances.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                }
                lStatus.Text = String.Format(CultureInfo.InvariantCulture, Resources.WMIForm_Items, lvInstances.Items.Count);
            }
        }

        [Localizable(false)]
        private void WMIForm_Load(object sender, EventArgs e)
        {
            CurrentWmiClass = "Win32_OperatingSystem";
            ActiveControl = lvInstances;
        }

        public void ShowStat(int classCount, int propCount, int methodCount)
        {
            lClasses.Text = String.Format(CultureInfo.InvariantCulture, Resources.WMIForm_Classes, classCount);
            lProps.Text = String.Format(CultureInfo.InvariantCulture, Resources.WMIForm_Properties, propCount);
            lMethods.Text = String.Format(CultureInfo.InvariantCulture, Resources.WMIForm_Methods, methodCount);
        }

        private static void DynObjAddProperty<T>(DynamicObject dynamicObj, PropertyData prop, string description, string category, bool isReadOnly)
        {
            if (prop.Value == null)
                dynamicObj.AddPropertyNull<T>(prop.Name, prop.Name, description, category, isReadOnly, null);
            else
                if (prop.IsArray)
                    dynamicObj.AddProperty(prop.Name, (T[])prop.Value, description, category, isReadOnly);
                else
                    dynamicObj.AddProperty(prop.Name, (T)prop.Value, description, category, isReadOnly);
        }

        public void MenuClassesOnClick(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if (menuItem != null)
                CurrentWmiClass = menuItem.Text;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public void UpdateClassesMenu()
        {
            menuClasses.Items.Clear();
            int count1 = WmiClassList.Instance.Classes.Count;
            int count2 = WmiClassList.Instance.ReadOnlyClasses.Count;
            foreach(var str in WmiClassList.Instance.Classes)
            {
                var menuItem = new ToolStripMenuItem {Text = str};
                menuItem.Click += MenuClassesOnClick;
                menuClasses.Items.Add(menuItem);
            }
            if (count1 > 0 && count2 > 0)
                menuClasses.Items.Add(new ToolStripSeparator());
            foreach (var str in WmiClassList.Instance.ReadOnlyClasses)
            {
                var menuItem = new ToolStripMenuItem { Text = str };
                menuItem.Click += MenuClassesOnClick;
                menuClasses.Items.Add(menuItem);
            }
            // TODO uncomment setup wmi-classes
            //if (Count1 + Count2 > 0)
            //    menuClasses.Items.Add(new ToolStripSeparator());
            //ToolStripMenuItem mSetup = new ToolStripMenuItem { Text = "Настроить..."};
            //mSetup.Click += mSetup_Click;
            //menuClasses.Items.Add(mSetup);
        }

        private bool m_MenuUpdated;

        private void menuClasses_Opening(object sender, CancelEventArgs e)
        {
            if (!WmiClassList.Instance.Loaded)
            {
                e.Cancel = true;
                return;
            }
            if (!m_MenuUpdated)
            {
                UpdateClassesMenu();
                m_MenuUpdated = true;
            }
            foreach (var menuItem in menuClasses.Items)
            {
                var mi = menuItem as ToolStripMenuItem;
                if (mi != null)
                    mi.Checked = mi.Text.Equals(CurrentWmiClass);
            }
        }

        private void PropGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            string propName = e.ChangedItem.Label;
            if (propName == null) return;
            object propValue = e.ChangedItem.Value;
            string caption = String.Format(CultureInfo.InvariantCulture, Resources.WMIForm_EditingProperty, propName);
            string message = String.Format(CultureInfo.InvariantCulture, Resources.WMIForm_PropertyChanged_Message, m_Args.ComputerName, e.OldValue, propValue);
            try
            {
                // trying to change wmi property
                m_Presenter.WmiObject[propName] = propValue;
                m_Presenter.WmiObject.Put();

                // update computer comment if we changes Win32_OperatingSystme.Description
                if (CurrentWmiClass.Equals("Win32_OperatingSystem") && propName.Equals("Description"))
                    UpdateTitle();

                // property has been changed
                message += String.Format(CultureInfo.InvariantCulture, Resources.WMIForm_PropertyChanged_Success, propName);
                MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, WMIViewer.RightToLeft.Options);
            }
            catch(ManagementException ex)
            {
                // property not changed
                var dynObj = PropGrid.SelectedObject as DynamicObject;
                if (dynObj != null)
                    dynObj[propName] = e.OldValue;
                message += "\n\n" + ex.Message;
                if (ex.InnerException != null)
                    message += "\n\n" + ex.InnerException.Message;
                MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, WMIViewer.RightToLeft.Options);
            }
        }

        private void DoFocusedItemChanged()
        {
            if (FocusedItemChanged != null)
                FocusedItemChanged(this, EventArgs.Empty);
        }

        private void LVCompsOnItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
                DoFocusedItemChanged();
        }

        private void MenuCommandsOnOpening(object sender, CancelEventArgs e)
        {
            if (LV.FocusedItem != null)
                if (LV.FocusedItem.Selected)
                    DoFocusedItemChanged();
        }

        private void lvInstances_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                e.Handled = true;
            }
        }

        private void WMIForm_KeyDown(object sender, KeyEventArgs e)
        {
            // Esc, F10 - quit program
            if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.F10)
            {
                Close();
                e.Handled = true;
            }
            if (e.KeyCode == Keys.F2)
            {
                lClassName.ShowDropDown();
                e.Handled = true;
            }
            if (e.Control && e.KeyCode == Keys.R)
            {
                CurrentWmiClass = CurrentWmiClass;
                e.Handled = true;
            }
            // Ctrl+Left
            if (e.Control && e.KeyCode == Keys.Left)
            {
                PropGrid.Dock = DockStyle.Left;
                TheSplitter.Dock = DockStyle.Left;
                e.Handled = true;
            }
            // Ctrl+Right
            if (e.Control && e.KeyCode == Keys.Right)
            {
                PropGrid.Dock = DockStyle.Right;
                TheSplitter.Dock = DockStyle.Right;
                e.Handled = true;
            }
        }
    }

}
