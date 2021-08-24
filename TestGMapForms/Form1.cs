using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace TestGMapForms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Загрузка данных GMap
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gMapControl1_Load(object sender, EventArgs e)
        {
            gMapControl1.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
            gMapControl1.Position = new GMap.NET.PointLatLng(54, 88);
            gMapControl1.ShowCenter = false;
            gMapControl1.MinZoom = 1;
            gMapControl1.MaxZoom = 13;
            gMapControl1.Zoom = 5;
            gMapControl1.DragButton = MouseButtons.Right;

            mouseIsDown = false;
            IsMarkerEnter = false;
            currentMarker = null;

            var units = serviceDb.GetAll();
            var markers = LoadMarkers(units);

            gMapControl1.Overlays.Add(markers);
        }

        /// <summary>
        /// Загрузка данных на GMap из БД
        /// </summary>
        /// <param name="units"></param>
        /// <returns></returns>
        private GMapOverlay LoadMarkers(List<Unit> units)
        {
            var markers = new GMapOverlay("Маркеры");

            foreach (var unit in units)
            {
                var marker = new GMarkerGoogle(
                    new GMap.NET.PointLatLng(unit.PositionX, unit.PositionY), GMarkerGoogleType.red)
                {
                    ToolTipText = unit.Name,
                    Tag = unit.Id.ToString()
                };

                markers.Markers.Add(marker);
            }

            return markers;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        private void gMapControl1_OnMarkerEnter(GMapMarker item)
        {
            if (currentMarker == null)
            {
                currentMarker = Convert.ToInt32(item.Tag);
                IsMarkerEnter = true;
            }
        }

        /// <summary>
        /// Отпустили маркер
        /// </summary>
        /// <param name="item"></param>
        private void gMapControl1_OnMarkerLeave(GMapMarker item)
        {
            if (!mouseIsDown)
            {
                currentMarker = null;
                IsMarkerEnter = false;
            }
        }

        /// <summary>
        /// Координаты Конечной точки маркера
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gMapControl1_MouseDown(object sender, MouseEventArgs e)
        {
            mouseIsDown = true;
            mouseDownPoint = new Point(e.Location.X, e.Location.Y);
        }

        /// <summary>
        /// Поднятие маркера
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gMapControl1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseIsDown = false;
            if (currentMarker != null)
            {
                var marker = GetMarkerById(currentMarker);
                serviceDb.ChangePosition(Convert.ToInt32(marker.Tag), marker.Position.Lat, marker.Position.Lng);
            }
        }

        /// <summary>
        /// Перемещение поднятого маркера
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gMapControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsMarkerEnter && mouseIsDown)
            {
                var marker = GetMarkerById(currentMarker);

                if (marker != null)
                {
                    var point = gMapControl1.FromLocalToLatLng(e.Location.X, e.Location.Y);
                    marker.Position = new GMap.NET.PointLatLng(point.Lat, point.Lng);
                }
            }
        }

        /// <summary>
        /// Получить данные маркера по Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private GMapMarker GetMarkerById(int? id)
        {
            return gMapControl1
                    .Overlays
                    .FirstOrDefault(x => x.Id == "Маркеры")
                    .Markers
                    .FirstOrDefault(m => Convert.ToInt32(m.Tag) == id);
        }
    }
}