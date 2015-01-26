﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GraphX;
using GraphX.Controls;
using ShowcaseApp.WPF.Models;
using Rect = GraphX.Measure.Rect;

namespace ShowcaseApp.WPF.Pages
{
    /// <summary>
    /// Interaction logic for DynamicGraph.xaml
    /// </summary>
    public partial class EditorGraph
    {
        /// <summary>
        /// tmp collection to speedup selected vertices search
        /// </summary>
        private readonly List<VertexControl> _selectedVertices = new List<VertexControl>();

        private EditorOperationMode _opMode = EditorOperationMode.Select;
        private VertexControl _ecFrom;


        public EditorGraph()
        {
            InitializeComponent();
            var dgLogic = new LogicCoreExample();
            graphArea.LogicCore = dgLogic;
            graphArea.VertexSelected += graphArea_VertexSelected;
           // addVertexButton.Click += addVertexButton_Click;
           // addEdgeButton.Click += addEdgeButton_Click;

            dgLogic.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.Custom;
            dgLogic.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.None;
            dgLogic.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.None;
            dgLogic.EdgeCurvingEnabled = true;
            

            //graphArea.MoveAnimation = AnimationFactory.CreateMoveAnimation(MoveAnimation.Move, TimeSpan.FromSeconds(0.5));
            //graphArea.MoveAnimation.Completed += MoveAnimation_Completed;
            //graphArea.VertexSelected += dg_Area_VertexSelected;
            
            

            zoomCtrl.IsAnimationDisabled = true;
            ZoomControl.SetViewFinderVisibility(zoomCtrl, Visibility.Visible);
            zoomCtrl.Zoom = 3;
            zoomCtrl.MinZoom = .5;
            zoomCtrl.MaxZoom = 50;
            zoomCtrl.MouseDown += zoomCtrl_MouseDown;
            var tb = new TextBlock() {Text = "AAAA"};
            graphArea.AddCustomChildControl(tb);
            GraphAreaBase.SetX(tb, 0);
            GraphAreaBase.SetY(tb, 0, true);
            graphArea.UpdateLayout();
            zoomCtrl.ZoomToFill();
            graphArea.RemoveCustomChildControl(tb);

            //zoomCtrl.ZoomToContent(new System.Windows.Rect(0,0, 500, 500));

            butDelete.Checked += ToolbarButton_Checked;
            butSelect.Checked += ToolbarButton_Checked;
            butEdit.Checked += ToolbarButton_Checked;

            butSelect.IsChecked = true;

        }

        void graphArea_VertexSelected(object sender, GraphX.Models.VertexSelectedEventArgs args)
        {
             if(args.MouseArgs.LeftButton == MouseButtonState.Pressed && _opMode == EditorOperationMode.Edit)
             {
                 CreateEdgeControl(args.VertexControl);
                 return;
             }
        }

        void zoomCtrl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //create vertices and edges only in Edit mode
            if(_opMode != EditorOperationMode.Edit) return;
            if(e.LeftButton == MouseButtonState.Pressed)
                CreateVertexControl(zoomCtrl.TranslatePoint(e.GetPosition(zoomCtrl), graphArea));
        }


        void ToolbarButton_Checked(object sender, RoutedEventArgs e)
        {
            if(butDelete.IsChecked == true && sender == butDelete)
            {
                butEdit.IsChecked = false;
                butSelect.IsChecked = false;
                zoomCtrl.Cursor = Cursors.Help;
                _opMode = EditorOperationMode.Delete;
                graphArea.SetVerticesDrag(false);
                return;
            }
            if (butEdit.IsChecked == true && sender == butEdit)
            {
                butDelete.IsChecked = false;
                butSelect.IsChecked = false;
                zoomCtrl.Cursor = Cursors.Pen;
                _opMode = EditorOperationMode.Edit;
                graphArea.SetVerticesDrag(false);
                return;
            }
            if (butSelect.IsChecked == true && sender == butSelect)
            {
                butEdit.IsChecked = false;
                butDelete.IsChecked = false;
                zoomCtrl.Cursor = Cursors.Hand;
                _opMode = EditorOperationMode.Select;
                graphArea.SetVerticesDrag(true, true);
                return;
            }
        }

        private void CreateVertexControl(Point position)
        {
            var data = new DataVertex("Vertex " + (graphArea.VertexList.Count + 1));
            graphArea.LogicCore.Graph.AddVertex(data);
            var vc = new VertexControl(data);
            graphArea.AddVertex(data, vc);
            GraphAreaBase.SetX(vc, position.X, true);
            GraphAreaBase.SetY(vc, position.Y, true);
        }

        private void CreateEdgeControl(VertexControl vc)
        {
            if(_ecFrom == null)
            {
                _ecFrom = vc;
                HighlightBehaviour.SetHighlighted(_ecFrom, true);
                return;
            }
            if(_ecFrom == vc) return;

            var data = new DataEdge((DataVertex)_ecFrom.Vertex, (DataVertex)vc.Vertex);
            graphArea.LogicCore.Graph.AddEdge(data);
            var ec = new EdgeControl(_ecFrom, vc, data);
            graphArea.InsertEdge(data, ec);

            HighlightBehaviour.SetHighlighted(_ecFrom, false);
            _ecFrom = null;
        }

   /*     #region Manual edge drawing

        private bool _isInEdMode;
        private PathGeometry _edGeo;
        private VertexControl _edVertex;
        private EdgeControl _edEdge;
        private DataVertex _edFakeDv;

        void dg_zoomctrl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isInEdMode || _edGeo == null || _edEdge == null || _edVertex == null || e.LeftButton != MouseButtonState.Pressed) return;
            //place point
            var pos = dg_zoomctrl.TranslatePoint(e.GetPosition(dg_zoomctrl), dg_Area);
            var lastseg = _edGeo.Figures[0].Segments[_edGeo.Figures[0].Segments.Count - 1] as PolyLineSegment;
            if (lastseg != null) lastseg.Points.Add(pos);
            _edEdge.SetEdgePathManually(_edGeo);
        }

        void dg_Area_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isInEdMode || _edGeo == null || _edEdge == null || _edVertex == null) return;
            var pos = dg_zoomctrl.TranslatePoint(e.GetPosition(dg_zoomctrl), dg_Area);
            var lastseg = _edGeo.Figures[0].Segments[_edGeo.Figures[0].Segments.Count - 1] as PolyLineSegment;
            if (lastseg != null) lastseg.Points[lastseg.Points.Count - 1] = pos;
            _edEdge.SetEdgePathManually(_edGeo);
        }

        void dg_Area_VertexSelectedForED(object sender, VertexSelectedEventArgs args)
        {
            if (!_isInEdMode) return;
            if (_edVertex == null) //select starting vertex
            {
                _edVertex = args.VertexControl;
                _edFakeDv = new DataVertex { ID = -666 };
                _edGeo = new PathGeometry(new PathFigureCollection { new PathFigure { IsClosed = false, StartPoint = _edVertex.GetPosition(), Segments = new PathSegmentCollection { new PolyLineSegment(new List<Point> { new Point() }, true) } } });
                var dedge = new DataEdge(_edVertex.Vertex as DataVertex, _edFakeDv);
                _edEdge = new EdgeControl(_edVertex, null, dedge) { ManualDrawing = true };
                dg_Area.AddEdge(dedge, _edEdge);
                dg_Area.LogicCore.Graph.AddVertex(_edFakeDv);
                dg_Area.LogicCore.Graph.AddEdge(dedge);
                _edEdge.SetEdgePathManually(_edGeo);
            }
            else if (!Equals(_edVertex, args.VertexControl)) //finish draw
            {
                _edEdge.Target = args.VertexControl;
                var dedge = _edEdge.Edge as DataEdge;
                if (dedge != null) dedge.Target = args.VertexControl.Vertex as DataVertex;
                var fig = _edGeo.Figures[0];
                var seg = fig.Segments[_edGeo.Figures[0].Segments.Count - 1] as PolyLineSegment;

                if (seg != null && seg.Points.Count > 0)
                {
                    var targetPos = _edEdge.Target.GetPosition();
                    var sourcePos = _edEdge.Source.GetPosition();
                    //get the size of the source
                    var sourceSize = new Size
                    {
                        Width = _edEdge.Source.ActualWidth,
                        Height = _edEdge.Source.ActualHeight
                    };
                    var targetSize = new Size
                    {
                        Width = _edEdge.Target.ActualWidth,
                        Height = _edEdge.Target.ActualHeight
                    };

                    var srcStart = seg.Points.Count == 0 ? fig.StartPoint : seg.Points[0];
                    var srcEnd = seg.Points.Count > 1 ? (seg.Points[seg.Points.Count - 1] == targetPos ? seg.Points[seg.Points.Count - 2] : seg.Points[seg.Points.Count - 1]) : fig.StartPoint;
                    var p1 = GeometryHelper.GetEdgeEndpoint(sourcePos, new Rect(sourceSize), srcStart, _edEdge.Source.VertexShape);
                    var p2 = GeometryHelper.GetEdgeEndpoint(targetPos, new Rect(targetSize), srcEnd, _edEdge.Target.VertexShape);


                    fig.StartPoint = p1;
                    if (seg.Points.Count > 1)
                        seg.Points[seg.Points.Count - 1] = p2;
                }
                GeometryHelper.TryFreeze(_edGeo);
                _edEdge.SetEdgePathManually(new PathGeometry(_edGeo.Figures));
                _isInEdMode = false;
                ClearEdgeDrawing();
            }
        }


        void ClearEdgeDrawing()
        {
            _edGeo = null;
            if (_edFakeDv != null)
                dg_Area.LogicCore.Graph.RemoveVertex(_edFakeDv);
            _edFakeDv = null;
            _edVertex = null;
            _edEdge = null;
        }

        private void dg_butdraw_Click(object sender, RoutedEventArgs e)
        {
            if (!_isInEdMode)
            {
                if (dg_Area.VertexList.Count() < 2)
                {
                    MessageBox.Show("Please add more vertices before proceed with this action!", "Starting to draw custom edge...", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                MessageBox.Show("Please select any vertex to define edge starting point!", "Starting to draw custom edge...", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Edge drawing mode has been canceled!");
                if (_edEdge != null)
                    _edEdge.SetEdgePathManually(null);
                ClearEdgeDrawing();
            }
            _isInEdMode = !_isInEdMode;
        }

        #endregion

        #region Dragging example
        void dg_dragsource_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var data = new DataObject(typeof(object), new object());
            DragDrop.DoDragDrop(dg_dragsource, data, DragDropEffects.Link);
        }

        static void dg_Area_DragEnter(object sender, DragEventArgs e)
        {
            //don't show drag effect if we are on drag source or don't have any item of needed type dragged
            if (!e.Data.GetDataPresent(typeof(object)) || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        void dg_Area_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof (object))) return;
            //how to get dragged data by its type
            var pos = dg_zoomctrl.TranslatePoint(e.GetPosition(dg_zoomctrl), dg_Area);
            var data = new DataVertex("Vertex " + (dg_Area.VertexList.Count() + 1));
            dg_Area.LogicCore.Graph.AddVertex(data);
            var vc = new VertexControl(data);
            dg_Area.AddVertex(data, vc);
            GraphAreaBase.SetX(vc, pos.X);
            GraphAreaBase.SetY(vc, pos.Y, true);

        }

        #endregion


        private void SelectVertex(VertexControl vc)
        {
            if (_selectedVertices.Contains(vc))
            {
                _selectedVertices.Remove(vc);
                HighlightBehaviour.SetHighlighted(vc, false);
                DragBehaviour.SetIsTagged(vc, false);
            }
            else
            {
                _selectedVertices.Add(vc);
                HighlightBehaviour.SetHighlighted(vc, true);
                DragBehaviour.SetIsTagged(vc, true);
            }
        }

        void graphArea_VertexSelected(object sender, VertexSelectedEventArgs args)
        {
            if (args.MouseArgs.LeftButton == MouseButtonState.Pressed)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    SelectVertex(args.VertexControl);
            }
            else if (args.MouseArgs.RightButton == MouseButtonState.Pressed)
            {
                args.VertexControl.ContextMenu = new ContextMenu();
                var mi = new MenuItem { Header = "Delete item", Tag = args.VertexControl };
                mi.Click += mi_Click;
                args.VertexControl.ContextMenu.Items.Add(mi);
            }
        }

        void mi_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;
            var vc = menuItem.Tag as VertexControl;
            if (vc != null) SafeRemoveVertex(vc, true);
        }

        void dg_remedge_Click(object sender, RoutedEventArgs e)
        {
            if (!dg_Area.EdgesList.Any()) return;
            dg_Area.LogicCore.Graph.RemoveEdge(dg_Area.EdgesList.Last().Key);
            dg_Area.RemoveEdge(dg_Area.EdgesList.Last().Key);
        }

        private EditorMode EditorMode;
        void addEdgeButton_Click(object sender, RoutedEventArgs e)
        {
            if(EditorMode == EditorMode.AddEdge)
            {
                EditorMode = EditorMode.None;
                _selectedVertices.ForEach(SelectVertex);
                return;
            }
            EditorMode = EditorMode.AddEdge;
            
        }

        private void SafeRemoveVertex(VertexControl vc, bool removeFromSelected = false)
        {
            //remove all adjacent edges
            foreach (var ec in dg_Area.GetRelatedControls(vc, GraphControlType.Edge, EdgesType.All).OfType<EdgeControl>()) {
                dg_Area.LogicCore.Graph.RemoveEdge(ec.Edge as DataEdge);
                dg_Area.RemoveEdge(ec.Edge as DataEdge);
            }
            dg_Area.LogicCore.Graph.RemoveVertex(vc.Vertex as DataVertex);
            dg_Area.RemoveVertex(vc.Vertex as DataVertex);
            if (removeFromSelected && _selectedVertices.Contains(vc))
                _selectedVertices.Remove(vc);
            dg_zoomctrl.ZoomToFill();

        }

        void addVertexButton_Click(object sender, RoutedEventArgs e)
        {
            var data = new DataVertex();
            ThemedDataStorage.FillDataVertex(data);

            dg_Area.LogicCore.Graph.AddVertex(data);
            dg_Area.AddVertex(data, new VertexControl(data));

            //we have to check if there is only one vertex and set coordinates manulay 
            //because layout algorithms skip all logic if there are less than two vertices
            if (dg_Area.VertexList.Count() == 1)
                dg_Area.VertexList.First().Value.SetPosition(0, 0);
            else dg_Area.RelayoutGraph(true);
            dg_zoomctrl.ZoomToFill();
        }
        */

    }

    public enum EditorMode
    {
        None = 0,
        AddEdge,
        AddVertex
    }
}
