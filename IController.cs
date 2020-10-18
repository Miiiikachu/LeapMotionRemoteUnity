/******************************************************************************
 * Copyright (C) Ultraleap, Inc. 2011-2020.                                   *
 *                                                                            *
 * Use subject to the terms of the Apache License 2.0 available at            *
 * http://www.apache.org/licenses/LICENSE-2.0, or another agreement           *
 * between Ultraleap and you, your company or other organization.             *
 ******************************************************************************/

using System;

namespace Leap {
    public interface IController : IDisposable {
        /// <summary>
        /// In most cases you should get Frame objects using the LeapProvider.CurrentFrame
        /// property. The data in Frame objects taken directly from a Leap.Controller instance
        /// is still in the Leap Motion frame of reference and will not match the hands
        /// displayed in a Unity scene.
        /// 
        /// Returns a frame of tracking data from the Leap Motion software. Use the optional
        /// history parameter to specify which frame to retrieve. Call frame() or
        /// frame(0) to access the most recent frame; call frame(1) to access the
        /// previous frame, and so on. If you use a history value greater than the
        /// number of stored frames, then the controller returns an empty frame.
        /// 
        /// @param history The age of the frame to return, counting backwards from
        /// the most recent frame (0) into the past and up to the maximum age (59).
        /// @returns The specified frame; or, if no history parameter is specified,
        /// the newest frame. If a frame is not available at the specified history
        /// position, an invalid Frame is returned.
        /// @since 1.0
        /// </summary>
        Frame Frame(int history = 0);

        /// <summary>
        /// Returns the frame object with all hands transformed by the specified
        /// transform matrix.
        /// </summary>
        Frame GetTransformedFrame(LeapTransform trs, int history = 0);

        /// <summary>
        /// Returns the Frame at the specified time, interpolating the data between existing frames, if necessary.
        /// </summary>
        Frame GetInterpolatedFrame(Int64 time);

        /// <summary>
        /// Requests setting a policy.
        ///  
        /// A request to change a policy is subject to user approval and a policy 
        /// can be changed by the user at any time (using the Leap Motion settings dialog). 
        /// The desired policy flags must be set every time an application runs. 
        ///  
        /// Policy changes are completed asynchronously and, because they are subject 
        /// to user approval or system compatibility checks, may not complete successfully. Call 
        /// Controller.IsPolicySet() after a suitable interval to test whether 
        /// the change was accepted. 
        /// @since 2.1.6 
        /// </summary>
        void SetPolicy(Controller.PolicyFlag policy);

        /// <summary>
        /// Requests clearing a policy.
        /// 
        /// Policy changes are completed asynchronously and, because they are subject
        /// to user approval or system compatibility checks, may not complete successfully. Call
        /// Controller.IsPolicySet() after a suitable interval to test whether
        /// the change was accepted.
        /// @since 2.1.6
        /// </summary>
        void ClearPolicy(Controller.PolicyFlag policy);

        /// <summary>
        /// Gets the active setting for a specific policy.
        /// 
        /// Keep in mind that setting a policy flag is asynchronous, so changes are
        /// not effective immediately after calling setPolicyFlag(). In addition, a
        /// policy request can be declined by the user. You should always set the
        /// policy flags required by your application at startup and check that the
        /// policy change request was successful after an appropriate interval.
        /// 
        /// If the controller object is not connected to the Leap Motion software, then the default
        /// state for the selected policy is returned.
        ///
        /// @since 2.1.6
        /// </summary>
        bool IsPolicySet(Controller.PolicyFlag policy);

        /// <summary>
        /// Returns a timestamp value as close as possible to the current time.
        /// Values are in microseconds, as with all the other timestamp values.
        /// 
        /// @since 2.2.7
        /// </summary>
        long Now();

        /// <summary>
        /// Reports whether this Controller is connected to the Leap Motion service and
        /// the Leap Motion hardware is plugged in.
        /// 
        /// When you first create a Controller object, isConnected() returns false.
        /// After the controller finishes initializing and connects to the Leap Motion
        /// software and if the Leap Motion hardware is plugged in, isConnected() returns true.
        /// 
        /// You can either handle the onConnect event using a Listener instance or
        /// poll the isConnected() function if you need to wait for your
        /// application to be connected to the Leap Motion software before performing some other
        /// operation.
        /// 
        /// @since 1.0
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Returns a Config object, which you can use to query the Leap Motion system for
        /// configuration information.
        /// 
        /// @since 1.0
        /// </summary>
        Config Config { get; }

        /// <summary>
        /// The list of currently attached and recognized Leap Motion controller devices.
        /// 
        /// The Device objects in the list describe information such as the range and
        /// tracking volume.
        /// 
        /// Currently, the Leap Motion Controller only allows a single active device at a time,
        /// however there may be multiple devices physically attached and listed here.  Any active
        /// device(s) are guaranteed to be listed first, however order is not determined beyond that.
        /// 
        /// @since 1.0
        /// </summary>
        DeviceList Devices { get; }

        event EventHandler<ConnectionEventArgs> Connect;
        event EventHandler<ConnectionLostEventArgs> Disconnect;
        event EventHandler<FrameEventArgs> FrameReady;
        event EventHandler<DeviceEventArgs> Device;
        event EventHandler<DeviceEventArgs> DeviceLost;
        event EventHandler<DeviceFailureEventArgs> DeviceFailure;
        event EventHandler<LogEventArgs> LogMessage;

        //new
        event EventHandler<PolicyEventArgs> PolicyChange;
        event EventHandler<ConfigChangeEventArgs> ConfigChange;
        event EventHandler<DistortionEventArgs> DistortionChange;
        event EventHandler<ImageEventArgs> ImageReady;
        event EventHandler<PointMappingChangeEventArgs> PointMappingChange;
        event EventHandler<HeadPoseEventArgs> HeadPoseChange;

        // LeapMotion forgotten fields to allow real usage of IController...

        /// <summary>
        /// Returns the timestamp of a recent tracking frame.  Use the
        /// optional history parameter to specify how many frames in the past
        /// to retrieve the timestamp.  Leave the history parameter as
        /// it's default value to return the timestamp of the most recent
        /// tracked frame.
        /// </summary>
        long FrameTimestamp(int history = 0);


        /// <summary>
        /// This is a special variant of GetInterpolatedFrameFromTime, for use with special
        /// features that only require the position and orientation of the palm positions, and do
        /// not care about pose data or any other data.
        /// 
        /// You must specify the id of the hand that you wish to get a transform for.  If you specify
        /// an id that is not present in the interpolated frame, the output transform will be the
        /// identity transform.
        /// </summary>
        void GetInterpolatedLeftRightTransform(Int64 time,
            Int64 sourceTime,
            int leftId,
            int rightId,
            out LeapTransform leftTransform,
            out LeapTransform rightTransform);

        void GetInterpolatedFrameFromTime(Frame toFill, Int64 time, Int64 sourceTime);

        /// <summary>
        /// Fills the Frame with data taken at the specified time, interpolating the data between existing frames, if necessary.
        /// </summary>
        void GetInterpolatedFrame(Frame toFill, Int64 time);

        /// <summary>
        /// Identical to Frame(history) but instead of constructing a new frame and returning
        /// it, the user provides a frame object to be filled with data instead.
        /// </summary>
        void Frame(Frame toFill, int history = 0);

        /// <summary>
        /// Starts the connection.
        /// 
        /// A connection starts automatically when created, but you can
        /// use this function to restart the connection after stopping it.
        /// 
        /// @since 3.0
        /// </summary>
        void StartConnection();

        /// <summary>
        /// Stops the connection.
        /// 
        /// No more frames or other events are received from a stopped connection. You can
        /// restart with StartConnection().
        /// 
        /// @since 3.0
        /// </summary>
        void StopConnection();

        /// <summary>
        /// Dispatched whenever a thread ends a profiling block.  The event is always
        /// dispatched from the thread itself.
        /// 
        /// The event data will contain the name of the profiling block.
        /// 
        /// @since 4.0
        /// </summary>
        event Action<EndProfilingBlockArgs> EndProfilingBlock;

        /// <summary>
        /// Dispatched whenever a thread enters a profiling block.  The event is always
        /// dispatched from the thread itself.
        /// 
        /// The event data will contain the name of the profiling block.
        /// 
        /// @since 4.0
        /// </summary>
        event Action<BeginProfilingBlockArgs> BeginProfilingBlock;

        /// <summary>
        /// Dispatched whenever a thread is finished profiling.  The event is always
        /// dispatched from the thread itself.
        /// 
        /// @since 4.0
        /// </summary>
        event Action<EndProfilingForThreadArgs> EndProfilingForThread;

        /// <summary>
        /// Dispatched whenever a thread wants to start profiling for a custom thread.
        /// The event is always dispatched from the thread itself.
        /// 
        /// The event data will contain the name of the thread, as well as an array of
        /// all possible profiling blocks that could be entered on that thread.
        /// 
        /// @since 4.0
        /// </summary>
        event Action<BeginProfilingForThreadArgs> BeginProfilingForThread;

        /// <summary>
        /// Reports whether your application has a connection to the Leap Motion
        /// daemon/service. Can be true even if the Leap Motion hardware is not available. 
        /// @since 1.2 
        /// </summary>
        bool IsServiceConnected { get; }
    }
}