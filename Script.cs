/*
 * R e a d m e
 * -----------
 * 
 * Hi :D
 */

readonly List<IMyCockpit> cockpits = new List<IMyCockpit>();
readonly List<IMyGyro> gyros = new List<IMyGyro>();
public Program()
{
    GetBlocks();
    Echo("Works"); // Works

    Runtime.UpdateFrequency = UpdateFrequency.Update1;
}
public void Main()
{
    int wait = 0;
    if (!cockpits[0].IsUnderControl) // check if cockpit is under control
    {
        SetGyrosOverrideOFF();
        Runtime.UpdateFrequency = UpdateFrequency.Update100; // change to 1 tick every 1.2 seconds
        if (wait++ > 360) //wait 10 mins before shutting down
            Me.Enabled = false;
    }
    else // only run if cockpit is under control
    {
        Runtime.UpdateFrequency = UpdateFrequency.Update1; // change to 60 ticks per second

        Vector3D gravityVec = Vector3D.Normalize(cockpits[0].GetNaturalGravity()); // gravity *world*
        Vector3D gravityShip = Vector3D.TransformNormal(gravityVec, MatrixD.Transpose(cockpits[0].WorldMatrix)); // gravity *ship*

        Vector3D horizon_right = gravityVec.Cross(cockpits[0].WorldMatrix.Backward); // horizon right *world* perpendicular to ship forward
        Vector3D horizon_rightShip = Vector3D.TransformNormal(horizon_right, MatrixD.Transpose(cockpits[0].WorldMatrix)); // horizon right *ship* perpendicular to ship forward

        var velocities = cockpits[0].GetShipVelocities();
        var velocitiesXY = Math.Abs(velocities.AngularVelocity.X) + Math.Abs(velocities.AngularVelocity.Y); // pitch + yaw speed

        Vector3D control_vector = gravityShip * -cockpits[0].RotationIndicator.Y + horizon_rightShip * -cockpits[0].RotationIndicator.X; // Mouse.X Pitch around horizon right. Mouse.Y Yaw around gravity
        control_vector.Z += horizon_rightShip.Y * 5 * Math.Max(velocitiesXY, 1) + cockpits[0].RollIndicator * 5; // force roll correction when necessary

        GyroControl(control_vector);
    }
}
void GyroControl(Vector3D Inputs)
{
    Vector3D rotShip = Vector3D.TransformNormal(Inputs, cockpits[0].WorldMatrix);
    foreach (var gyro in gyros)
    {
        Vector3D gyro_Rot = Vector3D.TransformNormal(rotShip, Matrix.Transpose(gyro.WorldMatrix));

        gyro.GyroOverride = true;
        gyro.Pitch = (float)gyro_Rot.X;
        gyro.Yaw = (float)gyro_Rot.Y;
        gyro.Roll = (float)gyro_Rot.Z;
    }
}
void SetGyrosOverrideOFF()
{
    foreach (var gyro in gyros)
        gyro.GyroOverride = false;
}
void GetBlocks()
{
    GridTerminalSystem.GetBlocksOfType(gyros);
    GridTerminalSystem.GetBlocksOfType(cockpits);
}
