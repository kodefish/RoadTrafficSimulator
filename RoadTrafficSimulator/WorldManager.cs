using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoadTrafficSimulator.Simulator;
using RoadTrafficSimulator.Graphics;
using RoadTrafficSimulator.XNAHelpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RoadTrafficSimulator
{
    // Acts as a bridge between RoadTrafficSimulator and Monogame
    class WorldManager
    {
        private Game game;
        private SimulatorWorld world;
        private RTSRenderer rtsRenderer;

        public WorldManager(Game game)
        {
            this.game = game;
            world = new SimulatorWorld();
            rtsRenderer = new RTSRenderer();
        }

        public void Initialize()
        {
            // GenerateSquare();
            GenerateStrip();
        }

        private void GenerateSquare()
        {
            // Set scale to 100 so you actually see smth, but don't have to do any hard math yourself
            // ain't that aboslutely beautiful ?
            rtsRenderer.Scale = 100;
            // Generate the world map here
            Intersection xnaIntersection1 = new Intersection(new DataStructures.Vector2(1, 1));
            FourWayIntersection intersection1 = new FourWayIntersection(xnaIntersection1);

            Intersection xnaIntersection2 = new Intersection(new DataStructures.Vector2(1, 6));
            FourWayIntersection intersection2 = new FourWayIntersection(xnaIntersection2);

            Intersection xnaIntersection3 = new Intersection(new DataStructures.Vector2(6, 1));
            FourWayIntersection intersection3 = new FourWayIntersection(xnaIntersection3);

            Intersection xnaIntersection4 = new Intersection(new DataStructures.Vector2(6, 6));
            FourWayIntersection intersection4 = new FourWayIntersection(xnaIntersection4);


            Road road12 = new Road(intersection1, intersection2, 0, 1, RoadOrientation.Vertical);
            Road road13 = new Road(intersection1, intersection3, 0, 1, RoadOrientation.Horizontal);
            Road road24 = new Road(intersection2, intersection4, 0, 1, RoadOrientation.Horizontal);
            Road road34 = new Road(intersection3, intersection4, 0, 1, RoadOrientation.Vertical);

            // Add the stuff
            world.AddIntersection(intersection1);
            world.AddIntersection(intersection2);
            world.AddIntersection(intersection3);
            world.AddIntersection(intersection4);

            world.AddRoad(road12);
            world.AddRoad(road13);
            world.AddRoad(road24);
            world.AddRoad(road34);
        }

        private void GenerateStrip()
        {
            float scale = 5;
            rtsRenderer.Scale = scale;
            float displayWidth = game.GraphicsDevice.DisplayMode.Width / scale;
            float displayHeight = game.GraphicsDevice.DisplayMode.Height / scale;

            float padding = 42;

            Intersection xnaIntersection1 = new Intersection(new DataStructures.Vector2(padding, displayHeight / 2));
            FourWayIntersection intersection1 = new FourWayIntersection(xnaIntersection1);

            Intersection xnaIntersection2 = new Intersection(new DataStructures.Vector2(displayWidth - padding, displayHeight / 2));
            FourWayIntersection intersection2 = new FourWayIntersection(xnaIntersection2);

            Road road = new Road(intersection1, intersection2, 5, 6, RoadOrientation.Horizontal);

            world.AddIntersection(intersection1);
            world.AddIntersection(intersection2);
            world.AddRoad(road);
        }

        public void LoadContent(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            // Load content for renderer stuff
            rtsRenderer.LoadContent(graphicsDevice, spriteBatch);
        }

        public void Draw()
        {
            // Draw the stuff
            Debug.WriteLine("Drawing world!");
            Debug.WriteLine("Intersections:");
            foreach (FourWayIntersection intersection in world.Intersections) rtsRenderer.DrawIntersection(intersection);
            Debug.WriteLine("Roads:");
            foreach (Road road in world.Roads) rtsRenderer.DrawRoad(road);
            Debug.WriteLine("\n");
        }

        public void Update(GameTime gameTime)
        {
            //  Update the stuff
            // world.Update((float) gameTime.ElapsedGameTime.TotalSeconds);
        }
    }
}
