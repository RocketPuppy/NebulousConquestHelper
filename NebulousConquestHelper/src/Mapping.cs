﻿using System;
using System.Xml.Serialization;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace NebulousConquestHelper
{
    public class Mapping
    {
        public const int CANVAS_LENGTH = 1024;
        public const int CANVAS_HEIGHT = 1024;
        public const int PIXELS_PER_AU = 128;
        private const int DIAMETER_STAR = 32;
        private const int DIAMETER_PLANET = 16;
        private const int DIAMETER_STATION = 8;

        private static Point Centre()
        {
            return new Point(CANVAS_LENGTH / 2, CANVAS_HEIGHT / 2);
        }

        private static Point Centre(int length, int height)
        {
            return new Point(length / 2, height / 2);
        }

        private static Rectangle RectangleAround(Point point, int size)
        {
            return new Rectangle(point.X - (size / 2), point.Y - (size / 2), size, size);
        }

        private static Rectangle RectangleAround(Point point, int length, int height)
        {
            return new Rectangle(point.X - (length / 2), point.Y - (height / 2), length, height);
        }
        
        private static Point PixelsFromAU(PointF point)
        {
            float x = point.X * PIXELS_PER_AU;
            if (point.X % 1 > 0.5) x++;
            float y = point.Y * PIXELS_PER_AU;
            if (point.Y % 1 > 0.5) y++;
            return new Point((int)((CANVAS_LENGTH / 2) + x), (int)((CANVAS_LENGTH / 2) + y));
        }

        private static void DrawCaption(Graphics map, string caption, Point textPos, int offset = 0)
        {
            int textX = (int)(textPos.X + offset + 2);
            int textY = textPos.Y - 8;
            /*
            map.FillRectangle(
                Brushes.Navy,
                new Rectangle(textX, textY - 1, caption.Length * 6, 18)
            );
            */
            map.DrawString(
                caption,
                SystemFonts.IconTitleFont,
                Brushes.White,
                new Point(textX, textY)
            );
        }

        private static Brush GetColourForType(Location.LocationSubType type)
        {
            switch (type)
            {
                case Location.LocationSubType.PlanetHabitable:
                    return Brushes.Green;
                case Location.LocationSubType.PlanetGaseous:
                    return Brushes.Purple;
                case Location.LocationSubType.PlanetBarren:
                    return Brushes.LightGray;
                case Location.LocationSubType.StationMining:
                    return Brushes.OrangeRed;
                case Location.LocationSubType.StationFactoryParts:
                    return Brushes.White;
                case Location.LocationSubType.StationFactoryRestores:
                    return Brushes.White;
                case Location.LocationSubType.StationSupplyDepot:
                    return Brushes.Aqua;
            }

            return Brushes.Black;
        }

        public static void CreateSystemMap(string fileName, System star, int daysFromStart = 0, bool showStations = false, bool showTypes = false)
        {
            Image img = Image.FromFile(Helper.DATA_FOLDER_PATH + "canvas.png");
            Graphics map = Graphics.FromImage(img);

            map.Clear(Color.Navy);

            List<Belt> belts = star.SurroundingBelts;
            belts.Sort();

            foreach (Belt belt in belts)
            {
                Brush beltColour = belt.NearEdgeDistanceAU <= 2.00f ? Brushes.DarkGoldenrod : Brushes.LightSkyBlue;
                map.FillEllipse(beltColour, RectangleAround(Centre(), (int)(belt.FarEdgeDistanceAU * PIXELS_PER_AU * 2)));
                map.FillEllipse(Brushes.Navy, RectangleAround(Centre(), (int)(belt.NearEdgeDistanceAU * PIXELS_PER_AU * 2)));
            }

            map.FillEllipse(Brushes.LightGoldenrodYellow, RectangleAround(Centre(), DIAMETER_STAR));
            
            foreach (Location planet in star.OrbitingLocations)
            {
                map.DrawEllipse(Pens.Blue, RectangleAround(Centre(), (int)(planet.OrbitalDistanceAU * PIXELS_PER_AU * 2)));
                Point planetPos = PixelsFromAU(planet.GetCoordinates(daysFromStart));
                Brush planetColour = planet.ControllingTeam == Game.ConquestTeam.GreenTeam ? Brushes.Green : Brushes.OrangeRed;
                if (showTypes) planetColour = GetColourForType(planet.SubType);
                map.FillEllipse(planetColour, RectangleAround(planetPos, DIAMETER_PLANET));

                foreach (Location station in planet.LagrangeLocations)
                {
                    Point stationPos = PixelsFromAU(station.GetCoordinates(daysFromStart));
                    Brush stationColour = station.ControllingTeam == Game.ConquestTeam.GreenTeam ? Brushes.Green : Brushes.OrangeRed;
                    if (showTypes) stationColour = GetColourForType(station.SubType);
                    map.FillRectangle(stationColour, RectangleAround(stationPos, DIAMETER_STATION));
                }
            }

            foreach (Belt belt in belts)
            {
                if (!showStations) DrawCaption(map, belt.Name, new Point((int)(belt.FarEdgeDistanceAU * PIXELS_PER_AU + (CANVAS_LENGTH / 2)), CANVAS_HEIGHT / 2));
            }

            foreach (Location planet in star.OrbitingLocations)
            {
                Point planetPos = PixelsFromAU(planet.GetCoordinates(daysFromStart));
                DrawCaption(map, planet.Name, planetPos, DIAMETER_PLANET / 2);

                foreach (Location station in planet.LagrangeLocations)
                {
                    Point stationPos = PixelsFromAU(station.GetCoordinates(daysFromStart));
                    if (showStations) DrawCaption(map, station.Name, stationPos, DIAMETER_STATION / 2);
                }
            }

            img.Save(Helper.DATA_FOLDER_PATH + fileName, ImageFormat.Png);
        }
    }
}