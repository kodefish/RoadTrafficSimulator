using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoadTrafficSimulator.Simulator;
using RoadTrafficSimulator.Simulator.WorldEntities;
using RoadTrafficSimulator.Graphics;
using Vector2 = RoadTrafficSimulator.Simulator.DataStructures.LinAlg.Vector2;

namespace RoadTrafficSimulator
{
    // Acts as a bridge between RoadTrafficSimulator and Monogame
    class WorldManager
    {
        private Game game;
        private SimulatorWorld world;
        private RTSRenderer rtsRenderer;

        // Car adder
        private readonly int NUM_CARS;
        private Random rng;
        private readonly double CAR_ADDITION_WAIT_TIME = 5;
        private double lastCarAddedTime;

        public WorldManager(Game game)
        {
            this.game = game;
            world = new SimulatorWorld();
            rtsRenderer = new RTSRenderer();

            NUM_CARS = 50;
            rng = new Random(NUM_CARS);
            lastCarAddedTime = 0;
        }

        public void Initialize()
        {
            GenerateSquare();
            // GenerateStrip();
        }

        private void GenerateSquare()
        {
            float scale = 20;
            rtsRenderer.Scale = scale;
            float displayWidth = game.GraphicsDevice.DisplayMode.Width / scale;
            float displayHeight = game.GraphicsDevice.DisplayMode.Height / scale;

            // Generate the world map here
            Vector2 posIntersection1 = new Vector2(0.25f * displayWidth, 0.25f * displayHeight);
            FourWayIntersection intersection1 = new FourWayIntersection(posIntersection1);

            Vector2 posIntersection2 = new Vector2(0.25f * displayWidth, 0.75f * displayHeight);
            FourWayIntersection intersection2 = new FourWayIntersection(posIntersection2);

            Vector2 posIntersection3 = new Vector2(0.75f * displayWidth, 0.25f * displayHeight);
            FourWayIntersection intersection3 = new FourWayIntersection(posIntersection3);

            Vector2 posIntersection4 = new Vector2(0.75f * displayWidth, 0.75f * displayHeight);
            FourWayIntersection intersection4 = new FourWayIntersection(posIntersection4);


            Road road12 = new Road(ref intersection1, ref intersection2, 1, 1, RoadOrientation.Vertical, 120);
            Road road13 = new Road(ref intersection1, ref intersection3, 1, 1, RoadOrientation.Horizontal, 120);
            Road road24 = new Road(ref intersection2, ref intersection4, 1, 1, RoadOrientation.Horizontal, 120);
            Road road34 = new Road(ref intersection3, ref intersection4, 1, 1, RoadOrientation.Vertical, 120);

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
            float scale = 10;
            rtsRenderer.Scale = scale;
            float displayWidth = game.GraphicsDevice.DisplayMode.Width / scale;
            float displayHeight = game.GraphicsDevice.DisplayMode.Height / scale;

            float padding = displayWidth / 4;

            Vector2 posIntersection1 = new Vector2(padding, displayHeight / 2);
            FourWayIntersection intersection1 = new FourWayIntersection(posIntersection1);

            Vector2 posIntersection2 = new Vector2(displayWidth - padding, displayHeight / 2);
            FourWayIntersection intersection2 = new FourWayIntersection(posIntersection2);

            Road road = new Road(ref intersection1, ref intersection2, 0, 1, RoadOrientation.Horizontal, 30);

            world.AddIntersection(intersection1);
            world.AddIntersection(intersection2);
            world.AddRoad(road);
        }
        
        private void AddRandomCar(Random rng)
        {
            Road randomRoad = world.Roads[rng.Next(0, world.Roads.Count)];
            Lane[] lanes = rng.Next() % 2 == 0 ? randomRoad.NorthBoundLanes : randomRoad.SouthBoundLanes;
            if (lanes.Length > 0)
            {
                Lane randomLane = lanes[rng.Next(0, lanes.Length)];
                CarParams carParams;
                carParams.Mass = 500;
                carParams.CarWidth = 2;
                carParams.CarLength = 4;
                carParams.MaxSpeed = 92;
                carParams.MaxAccleration = 1.97f;
                carParams.BrakingDeceleration = 4.20f;

                Car car = new Car(carParams, randomLane, (float)rng.NextDouble());
                world.AddCar(car);
            }
        }

        public void LoadContent(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            // Load content for renderer stuff
            rtsRenderer.LoadContent(graphicsDevice, spriteBatch);
        }

        public void Draw()
        {
            // Draw the stuff
            foreach (FourWayIntersection intersection in world.Intersections) rtsRenderer.DrawIntersection(intersection);
            foreach (Road road in world.Roads) rtsRenderer.DrawRoad(road);
            foreach (Car car in world.Cars) rtsRenderer.DrawCar(car);
        }

        public void Update(GameTime gameTime)
        {
            // Add a car every 3 seconds
            if (world.Cars.Count < NUM_CARS && gameTime.TotalGameTime.TotalSeconds - lastCarAddedTime > CAR_ADDITION_WAIT_TIME)
            {
                AddRandomCar(rng);
                lastCarAddedTime = gameTime.TotalGameTime.TotalSeconds;
            }
            //  Update the stuff
            world.Update((float) gameTime.ElapsedGameTime.TotalSeconds);
        }
    }
}
