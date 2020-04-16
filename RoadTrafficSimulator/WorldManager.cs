using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using RoadTrafficSimulator.Simulator;
using RoadTrafficSimulator.Simulator.WorldEntities;
using RoadTrafficSimulator.Simulator.DrivingLogic;
using RoadTrafficSimulator.Simulator.DrivingLogic.FiniteStateMachine;
using RoadTrafficSimulator.Graphics;
using Vector2 = RoadTrafficSimulator.Simulator.DataStructures.LinAlg.Vector2;
using RoadTrafficSimulator.Simulator.DataStructures.Geometry;

namespace RoadTrafficSimulator
{
    // Acts as a bridge between RoadTrafficSimulator and Monogame
    class WorldManager
    {
        private Game game;
        private SimulatorWorld world;
        private RTSRenderer rtsRenderer;

        // Car adder
        private int NUM_CARS;
        private Random rng;
        private readonly double CAR_ADDITION_WAIT_TIME = 1;
        private double lastCarAddedTime;

        public WorldManager(Game game)
        {
            this.game = game;
            world = new SimulatorWorld();
            rtsRenderer = new RTSRenderer();

            NUM_CARS = 0;
            rng = new Random(2);
            lastCarAddedTime = 0;
        }

        public void Initialize()
        {
            FillWorld();
            // TestNeighbors();
            // TestOvertaking();
            // TestIntersection();
        }

        private void GenerateGrid()
        {
            float scale = 10;
            rtsRenderer.Scale = scale;
            float displayWidth = game.GraphicsDevice.DisplayMode.Width / scale;
            float displayHeight = game.GraphicsDevice.DisplayMode.Height / scale;

            // Generate the world map here
            Vector2 posIntersection1 = new Vector2(0.25f * displayWidth, 0.25f * displayHeight);
            FourWayIntersection intersection1 = new FourWayIntersection(posIntersection1);

            Vector2 posIntersection2 = new Vector2(0.25f * displayWidth, 0.75f * displayHeight);
            FourWayIntersection intersection2 = new FourWayIntersection(posIntersection2);

            Vector2 posIntersection3 = new Vector2(0.5f * displayWidth, 0.25f * displayHeight);
            FourWayIntersection intersection3 = new FourWayIntersection(posIntersection3);

            Vector2 posIntersection4 = new Vector2(0.5f * displayWidth, 0.75f * displayHeight);
            FourWayIntersection intersection4 = new FourWayIntersection(posIntersection4);

            Vector2 posIntersection5 = new Vector2(0.75f * displayWidth, 0.25f * displayHeight);
            FourWayIntersection intersection5 = new FourWayIntersection(posIntersection5);

            Vector2 posIntersection6 = new Vector2(0.75f * displayWidth, 0.75f * displayHeight);
            FourWayIntersection intersection6 = new FourWayIntersection(posIntersection6);


            Road road12 = new Road(ref intersection1, ref intersection2, 2, 2, 120);
            Road road13 = new Road(ref intersection1, ref intersection3, 2, 2, 120);
            Road road24 = new Road(ref intersection2, ref intersection4, 2, 2, 120);
            Road road34 = new Road(ref intersection3, ref intersection4, 2, 2, 120);
            Road road35 = new Road(ref intersection3, ref intersection5, 2, 2, 120);
            Road road46 = new Road(ref intersection4, ref intersection6, 2, 2, 120);
            Road road56 = new Road(ref intersection5, ref intersection6, 2, 2, 120);

            // Add the stuff
            world.AddIntersection(intersection1);
            world.AddIntersection(intersection2);
            world.AddIntersection(intersection3);
            world.AddIntersection(intersection4);
            world.AddIntersection(intersection5);
            world.AddIntersection(intersection6);

            world.AddRoad(road12);
            world.AddRoad(road13);
            world.AddRoad(road24);
            world.AddRoad(road34);
            world.AddRoad(road35);
            world.AddRoad(road46);
            world.AddRoad(road56);
        }

        private void GenerateHorizontalStrip()
        {
            float scale = 10;
            rtsRenderer.Scale = scale;
            float displayWidth = game.GraphicsDevice.DisplayMode.Width / scale;
            float displayHeight = game.GraphicsDevice.DisplayMode.Height / scale;

            float padding = displayWidth / 4;

            Vector2 posIntersection1 = new Vector2(padding, displayHeight / 2);
            FourWayIntersection intersection1 = new FourWayIntersection(posIntersection1);

            Vector2 posIntersection2 = new Vector2(displayWidth / 2, 0.8f * displayHeight);
            FourWayIntersection intersection2 = new FourWayIntersection(posIntersection2);

            Vector2 middle = new Vector2(displayWidth / 2, displayHeight / 2);
            FourWayIntersection intersectionMiddle = new FourWayIntersection(middle);

            Road road1 = new Road(ref intersection1, ref intersectionMiddle, 3, 3, 30);
            Road road2 = new Road(ref intersectionMiddle, ref intersection2, 3, 3, 30);

            world.AddIntersection(intersection1);
            world.AddIntersection(intersection2);
            world.AddIntersection(intersectionMiddle);
            world.AddRoad(road1);
            world.AddRoad(road2);
        }

        private void GenerateCross()
        {
            float scale = 10;
            rtsRenderer.Scale = scale;
            float displayWidth = game.GraphicsDevice.DisplayMode.Width / scale;
            float displayHeight = game.GraphicsDevice.DisplayMode.Height / scale;

            float paddingHor = displayWidth / 8;
            float paddingVer = displayHeight / 8;

            Vector2 posIntersection1 = new Vector2(paddingHor, displayHeight / 2);
            FourWayIntersection intersection1 = new FourWayIntersection(posIntersection1);

            Vector2 posIntersection2 = new Vector2(displayWidth / 2, paddingVer);
            FourWayIntersection intersection2 = new FourWayIntersection(posIntersection2);

            Vector2 posIntersection3 = new Vector2(displayWidth - paddingHor, displayHeight / 2);
            FourWayIntersection intersection3 = new FourWayIntersection(posIntersection3);

            Vector2 posIntersection4 = new Vector2(displayWidth / 2, displayHeight - paddingVer);
            FourWayIntersection intersection4 = new FourWayIntersection(posIntersection4);

            Vector2 middle = new Vector2(displayWidth / 2, displayHeight / 2);
            FourWayIntersection intersectionMiddle = new FourWayIntersection(middle);

            Road road1 = new Road(ref intersection1, ref intersectionMiddle, 3, 3, 30);
            Road road2 = new Road(ref intersection2, ref intersectionMiddle, 3, 3, 30);
            Road road3 = new Road(ref intersectionMiddle, ref intersection3, 3, 3, 30);
            Road road4 = new Road(ref intersectionMiddle, ref intersection4, 3, 3, 30);

            world.AddIntersection(intersection1);
            world.AddIntersection(intersection2);
            world.AddIntersection(intersection3);
            world.AddIntersection(intersection4);
            world.AddIntersection(intersectionMiddle);
            world.AddRoad(road1);
            world.AddRoad(road2);
            world.AddRoad(road3);
            world.AddRoad(road4);
        }

        private void TestIntersection() {
            GenerateCross();
            NUM_CARS = 50;
        }

        private void FillWorld() {
            GenerateGrid();
            NUM_CARS = 100;
        }

        private void TestNeighbors()
        {
            GenerateHorizontalStrip();
            CarParams carParams = CarParams.Car;
            
            Road r = world.Roads[0];
            Lane[] lanes = r.InLanes;
            Lane laneUp = lanes[2];
            Car cA = new Car(0, carParams, laneUp, 0.333f);
            Car cB = new Car(1, carParams, laneUp, 0.666f);

            Lane laneMiddle = lanes[1];
            Car c1 = new Car(2, carParams, laneMiddle, 0.25f);
            Car c2 = new Car(3, carParams, laneMiddle, 0.5f);
            Car c3 = new Car(4, carParams, laneMiddle, 0.75f);

            Lane laneDown = lanes[0];
            Car cI = new Car(5, carParams, laneDown, 0.333f);
            Car cJ = new Car(6, carParams, laneDown, 0.666f);

            world.AddCar(cA);
            world.AddCar(cB);
            world.AddCar(c1);
            world.AddCar(c2);
            world.AddCar(c3);
            world.AddCar(cI);
            world.AddCar(cJ);
            NUM_CARS = world.Cars.Count;
        }
        private void TestOvertaking()
        {
            GenerateHorizontalStrip();

            Road r = world.Roads[0];
            Lane[] lanes = r.InLanes;
            Lane laneUp = lanes[2];

            CarParams carParamsA = CarParams.Truck;
            Car cA = new Car(0, carParamsA, laneUp, 0.3f);
            
            CarParams carParamsB = CarParams.Car;
            Car cB = new Car(1, carParamsB, laneUp, 0.1f);

            world.AddCar(cA);
            world.AddCar(cB);

            NUM_CARS = world.Cars.Count;
        }

        private void AddRandomCar(Random rng)
        {
            // Try adding a car until one succeeds (adding may fail if the selected lane does not have enough free space)
            // Limit the number of tries, because otherwise we may loop forever (no lane may have enough space)
            bool added = false;
            int numTries = 0;

            while (!added && numTries++ < 2 * world.Roads.Count)
            {
                Road randomRoad = world.Roads[rng.Next(0, world.Roads.Count)];
                Lane[] lanes = rng.Next() % 2 == 0 ? randomRoad.OutLanes : randomRoad.InLanes;
                if (lanes.Length > 0)
                {
                    CarParams carParams;
                    if (rng.Next() % 4 != 0) carParams = CarParams.Car;
                    else carParams = CarParams.Truck;

                    Lane randomLane = lanes[rng.Next(0, lanes.Length)];
                    if (randomLane.FreeLaneSpace() > carParams.CarLength + IntelligentDriverModel.MIN_BUMPER_TO_BUMPER_DISTANCE)
                    {
                        // Offset to spawn the car into the lane, offset by carLength / 2
                        float offset = carParams.CarLength / (2 * randomLane.Path.Length);
                        Car car = new Car(world.Cars.Count, carParams, randomLane, offset);
                        world.AddCar(car);
                        added = true;
                    }
                }
            }
        }

        public void LoadContent(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, ContentManager content)
        {
            // Load content for renderer stuff
            rtsRenderer.LoadContent(graphicsDevice, spriteBatch, content);
        }

        public void Draw(GameTime gameTime)
        {
            // Draw the stuff
            foreach (FourWayIntersection intersection in world.Intersections) rtsRenderer.DrawIntersection(intersection);
            foreach (Road road in world.Roads) rtsRenderer.DrawRoad(road);
            foreach (Car car in world.Cars) rtsRenderer.DrawCar(car);

            // Print stats to console
            /*
            Debug.WriteLine("World Stats:");
            Debug.WriteLine("Num active cars: {0}", world.Cars.Count);
            Debug.WriteLine("Num roads: {0}", world.Roads.Count);
            Debug.WriteLine("Num intersections: {0}", world.Intersections.Count);
            Debug.WriteLine("FPS: {0}", 1 / (float)gameTime.ElapsedGameTime.TotalSeconds);
            */
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
            world.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }
    }
}
