namespace RoadTrafficSimulator.Simulator.DrivingLogic
{
    /// <summary>
    /// Simple implementation of a PID controller
    /// </summary>
    class PIDController
    {
        // Errors
        private float pError, iError, dError;

        // Coefficients
        private float kP, kI, kD;

        // Previous error
        private float previousError;

        /// <summary>
        /// PID controller with tuning params
        /// </summary>
        /// <param name="kP">Tuning parameter of proportional component</param>
        /// <param name="kI">Tuning parameter of integral component</param>
        /// <param name="kD">Tuning parameter of derivative component</param>
        public PIDController(float kP, float kI, float kD)
        {
            this.kP = kP;
            this.kI = kI;
            this.kD = kD;

            pError = 0;
            iError = 0;
            dError = 0;

            previousError = 0;
        }

        /// <summary>
        /// Update PID controller with new error (cross track error)
        /// </summary>
        /// <param name="error">Cross track error</param>
        public void UpdateError(float error, float dt)
        {
            pError = error;
            iError = iError + (error * dt);
            dError = (error - previousError) / dt;
            previousError = error;
        }

        /// <summary>
        /// Correction value as determined by PID
        /// </summary>
        /// <returns>Error correction</returns>
        public float PIDError()
        {
            return kP * pError + kI * iError + kD * dError;
        }


    }
    
}