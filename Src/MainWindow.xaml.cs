﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Banananana
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double cAutoSaveTime = 2.0;
        private const double cTitleUpdateTime = 1 / 30;

        private bool mDragging;
        private TaskControl mDraggedTask;
        private PileControl mDraggedPile;
        private Cursor mDragPreviousCursor;

        private string mInitialTitle;

        private DispatcherTimer mAutoSaveTimer;

        public MainWindow()
        {
            LoadWorkspace();

            InitializeComponent();

            // Init our UI
            foreach (Workspace.Pile pile in Workspace.Instance.Piles)
                AddNewPileControl(pile);

            mInitialTitle = Title;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(cTitleUpdateTime);
            timer.Tick += UpdateTitle;
            timer.Start();

            mAutoSaveTimer = new DispatcherTimer();
            mAutoSaveTimer.Interval = TimeSpan.FromSeconds(cAutoSaveTime);
            mAutoSaveTimer.Tick += AutoSave;
        }

        private void UpdateTitle(object sender, EventArgs e)
        {
            string suffix = "";
            if (Workspace.Instance.IsDirty)
            {
                if (!mAutoSaveTimer.IsEnabled)
                    mAutoSaveTimer.Start();
                suffix = " (modified)";
            }
            Title = mInitialTitle + suffix;
        }

        private void AutoSave(object sender, EventArgs e)
        {
            if (!Workspace.Instance.IsDirty)
                return;

            SaveWorkspace();
            SaveWindowSizeAndPosition();
            mAutoSaveTimer.Stop();
        }

        public IEnumerable<PileControl> PileControls
        {
            get
            {
                for (int i = 0; i < stackPanel.Children.Count - 1; ++i)
                    yield return stackPanel.Children[i] as PileControl;
            }
        }


        private PileControl AddNewPileControl(Workspace.Pile inPile)
        {
            PileControl pile_control = new PileControl(this, inPile);
            pile_control.VerticalAlignment = VerticalAlignment.Top;

            pile_control.OnDragTaskControlStarted += Pile_OnDragTaskStarted;
            pile_control.OnDragTaskControlMoved += Pile_OnDragTaskMoved;
            pile_control.OnDragTaskControlStopped += Pile_OnDragTaskStopped;

            pile_control.OnDragPileControlStarted += Pile_OnDragPileStarted;
            pile_control.OnDragPileControlMoved += Pile_OnDragPileMoved;
            pile_control.OnDragPileControlStopped += Pile_OnDragPileStopped;

            stackPanel.Children.Insert(stackPanel.Children.Count-1, pile_control);

            return pile_control;
        }


        public void DeletePileAndControl(PileControl inPileControl)
        {
            Workspace.Instance.RemovePile(inPileControl.Pile);
            stackPanel.Children.Remove(inPileControl);
        }



        private void Pile_OnDragPileStarted(PileControl inPile)
        {
            mDraggedPile = inPile;

            mDragPreviousCursor = Cursor;
            Cursor = Cursors.Hand;

            // Update dragging state of all piles
            foreach (PileControl pile in PileControls)
                pile.DragState = (pile == mDraggedPile) ? PileControl.EDragState.IsBeingDragged : PileControl.EDragState.IsNotBeingDragged;


            mDragging = true;
        }

        private void Pile_OnDragPileMoved(PileControl inPileControl, Point inPosition)
        {
            if (!mDragging)
                return;

            // Find out where to place our task
            Point mouse_pos = inPosition;

            // Determine pile to place task in
            double pile_width = (stackPanel.Children[0] as PileControl).Width; // Child 0 is always the header of the pile
            int num_piles = stackPanel.Children.Count - 1;

            int preferred_pile_ctrl_index = Math.Min((int)(mouse_pos.X / pile_width), num_piles - 1);
            int current_pile_ctrl_index = stackPanel.Children.IndexOf(inPileControl);

            // Move the pile to a different spot?
            if (current_pile_ctrl_index != preferred_pile_ctrl_index)
            {
                // Remove pile and control
                Workspace.Instance.RemovePileAt(current_pile_ctrl_index);
                stackPanel.Children.RemoveAt(current_pile_ctrl_index);

                // Insert pile and control at correct spot
                Workspace.Instance.InsertPile(preferred_pile_ctrl_index, inPileControl.Pile);
                stackPanel.Children.Insert(preferred_pile_ctrl_index, inPileControl);
            }
        }

        private void Pile_OnDragPileStopped(PileControl inPile)
        {
            if (!mDragging)
                return;

            Cursor = mDragPreviousCursor;

            // Update dragging state of all piles
            foreach (PileControl pile in PileControls)
                pile.DragState = PileControl.EDragState.NoDraggingActive;

            mDragging = false;
        }




        private void Pile_OnDragTaskStarted(TaskControl inTask)
        {
            mDraggedTask = inTask;

            //mDraggedTask.CaptureMouse();
            mDragPreviousCursor = Cursor;
            Cursor = Cursors.Hand;

            // Update dragging state of all tasks
            foreach (PileControl pile in PileControls)
                foreach (TaskControl task in pile.TaskControls)
                    task.DragState = (task == mDraggedTask) ? TaskControl.EDragState.IsBeingDragged : TaskControl.EDragState.IsNotBeingDragged;

            mDragging = true;
        }

        private void Pile_OnDragTaskMoved(TaskControl inTask, Point inPosition)
        {
            if (!mDragging)
                return;

            // Determine from which pile we're dragging
            int current_pile_index = -1;
            PileControl current_pile = null;
            for (int j = 0; j < stackPanel.Children.Count - 1; ++j)
            {
                PileControl pile = stackPanel.Children[j] as PileControl;
                if (pile.stackPanel.Children.Contains(mDraggedTask))
                {
                    current_pile_index = j;
                    current_pile = pile;
                    break;
                }
            }

            // Determine which task we're dragging
            int current_task_ctrl_index = current_pile.stackPanel.Children.IndexOf(mDraggedTask);

            // Find out where to place our task
            Point mouse_pos = inPosition;

            // Determine pile to place task in
            double pile_width = (stackPanel.Children[0] as PileControl).Width; // Child 0 is always the header of the pile
            int num_piles = stackPanel.Children.Count - 1;

            int preferred_pile_index = Math.Min((int)(mouse_pos.X / pile_width), num_piles-1);
            PileControl preferred_pile = stackPanel.Children[preferred_pile_index] as PileControl;

            // Determine task index we're trying to move our task to
            int preferred_task_ctrl_index = -1;

            if (preferred_pile.stackPanel.Children.Count >= 3)
            {
                if (preferred_pile_index == current_pile_index)
                {
                    double cur_top_y = mDraggedTask.TransformToAncestor(current_pile.stackPanel).Transform(new Point(0, 0)).Y;
                    double cur_h = mDraggedTask.ActualHeight;

                    // If our mouse is still within the task that we're dragging, no need to move it.
                    if (mouse_pos.Y >= cur_top_y && mouse_pos.Y < cur_top_y + cur_h)
                        preferred_task_ctrl_index = current_task_ctrl_index;
                }
                
                if (preferred_task_ctrl_index < 0)
                {
                    double top_y = preferred_pile.stackPanel.Children[2].TransformToAncestor(stackPanel).Transform(new Point(0, 0)).Y;
                    preferred_task_ctrl_index = 2;

                    for (int i = 2; i < preferred_pile.stackPanel.Children.Count; ++i)
                    {
                        UIElement element = preferred_pile.stackPanel.Children[i];
                        if (element == mDraggedTask)
                            continue;

                        double h = (preferred_pile.stackPanel.Children[i] as FrameworkElement).ActualHeight;

                        if (mouse_pos.Y < top_y + h && mouse_pos.Y >= top_y)
                            break;

                        top_y += h;
                        preferred_task_ctrl_index++;
                    }
                }
            }
            // Our destination pile is still empty, add task to bottom.
            else
            {
                preferred_task_ctrl_index = preferred_pile.stackPanel.Children.Count;
            }

            // Move dragged task to a different pile? Or move dragged task to different spot in same pile?
            if (current_pile_index != preferred_pile_index || current_task_ctrl_index != preferred_task_ctrl_index)
            {
                current_pile.MoveTaskControlToPileControl(mDraggedTask, preferred_pile, preferred_task_ctrl_index);
            }
        }


        private void Pile_OnDragTaskStopped(TaskControl inTask)
        {
            if (!mDragging)
                return;

            Cursor = mDragPreviousCursor;

            foreach (PileControl pile in PileControls)
                foreach (TaskControl task in pile.TaskControls)
                    task.DragState = TaskControl.EDragState.NoDraggingActive;

            mDragging = false;
        }


        private String GetWorkspaceFilename()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Banananana\\workspace.json";
        }


        private void LoadWorkspace()
        {
            Workspace.LoadFromFile(GetWorkspaceFilename());
        }


        private void SaveWorkspace()
        {
            Workspace.Instance.SaveToFile(GetWorkspaceFilename());
        }

        private void SaveWindowSizeAndPosition()
        {
            // https://stackoverflow.com/questions/847752/net-wpf-remember-window-size-between-sessions

            if (WindowState == WindowState.Maximized)
            {
                // Use the RestoreBounds as the current values will be 0, 0 and the size of the screen
                Properties.Settings.Default.WindowTop = RestoreBounds.Top;
                Properties.Settings.Default.WindowLeft = RestoreBounds.Left;
                Properties.Settings.Default.WindowHeight = RestoreBounds.Height;
                Properties.Settings.Default.WindowWidth = RestoreBounds.Width;
                Properties.Settings.Default.WindowMaximized = true;
            }
            else
            {
                Properties.Settings.Default.WindowTop = this.Top;
                Properties.Settings.Default.WindowLeft = this.Left;
                Properties.Settings.Default.WindowHeight = this.Height;
                Properties.Settings.Default.WindowWidth = this.Width;
                Properties.Settings.Default.WindowMaximized = false;
            }

            Properties.Settings.Default.HasWindowSizeAndPosition = true;

            Properties.Settings.Default.Save();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveWorkspace();
            SaveWindowSizeAndPosition();
        }

        private void AddPileRect_MouseEnter(object sender, MouseEventArgs e)
        {
            addPileRect.Opacity = 1.0;
        }

        private void AddPileRect_MouseLeave(object sender, MouseEventArgs e)
        {
            addPileRect.Opacity = 0.25;
        }

        private void AddPileRect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Workspace.Pile new_pile = new Workspace.Pile();
            Workspace.Instance.AddPile(new_pile);
            AddNewPileControl(new_pile);
        }

        public void ShowEditNotesWindow(Workspace.Task inTask)
        {
            EditNotesWindow window = new EditNotesWindow(Workspace.Instance, inTask);
            window.ShowDialog();
        }

        private void SetInitialWindowSizeAndPosition()
        {
            // Set initial window size and position
            if (Properties.Settings.Default.HasWindowSizeAndPosition)
            {
                this.Top = Properties.Settings.Default.WindowTop;
                this.Left = Properties.Settings.Default.WindowLeft;
                this.Width = Properties.Settings.Default.WindowWidth;
                this.Height = Properties.Settings.Default.WindowHeight;

                if (Properties.Settings.Default.WindowMaximized)
                    this.WindowState = WindowState.Maximized;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetInitialWindowSizeAndPosition();
        }
    }
}
