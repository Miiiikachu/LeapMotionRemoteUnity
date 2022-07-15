using System;
using System.Collections.Generic;
using System.Linq;
using Leap;
using Leap.Unity;
using LeapInternal;
using Newtonsoft.Json.Linq;
using Plugins.LeapMotion;
using UnityEngine;
using WebSocketSharp;

/// <summary>
/// The RemoteController class do the same as the Controller class
/// but with a remote controller through the WebSocket API of Leap Motion
/// </summary>

    public class RemoteController : IController
    {
        readonly WebSocket _webSocket;
        Controller.PolicyFlag _policy;

        CircularBuffer<Frame> _frames = new CircularBuffer<Frame>(60); // size of 60 like the hardcoded Leap stuff
        Frame _currentFrame = new Frame();

        public RemoteController(string url)
        {
            Debug.Log(url);
            _webSocket = new WebSocket(url);
            _webSocket.OnOpen += OnOpen;
            _webSocket.OnMessage += OnMessage;
            _webSocket.OnError += OnError;
            _webSocket.OnClose += OnClose;
            _webSocket.Connect();
            string command = "{\"background\": true}";
            _webSocket.Send(command);
    }

        void OnClose(object sender, CloseEventArgs e)
        {
            Debug.Log($"OnClose {sender} {e.Code} {e.Reason}");
            Disconnect?.Invoke(this, new ConnectionLostEventArgs());
        }

        void OnError(object sender, ErrorEventArgs e)
        {
            Debug.Log($"OnError {sender} {e.Exception} {e.Message}");
        }

        void OnMessage(object sender, MessageEventArgs e)
        {
            //Debug.Log($"OnMessage {sender} {e.Data}");

            JObject root = JObject.Parse(e.Data);

            if (root.ContainsKey("version"))
            {
                // {"serviceVersion":"2.3.1+33747", "version":6}
            }
            else if (root.ContainsKey("event"))
            { // Event
                Debug.Log($"OnMessage {sender} {e.Data}");
                /*
                 * {
                 *    "event": {
                 *      "state": {
                 *        "attached": false,
                 *        "id": "NNNNNNNNNNN",
                 *        "streaming": false,
                 *        "type": "peripheral"
                 *      },
                 *      "type": "deviceEvent"
                 *    }
                 * }
                 *
                 * example: {"event":{"state":{"attached":true,"id":"LP95597022824","streaming":true,"type":"Peripheral"},"type":"deviceEvent"}}
                 */
                // TODO dispatch events and keep an internal state to reflect the remote one
                if (root["event"].Value<string>("type") == "deviceEvent")
                {
                    Devices.Add(new Device(
                        deviceHandle: IntPtr.Zero,
                        internalHandle: IntPtr.Zero,
                        horizontalViewAngle: 0,
                        verticalViewAngle: 0,
                        range: 0,
                        baseline: 0,
                        type: Leap.Device.DeviceType.TYPE_PERIPHERAL, // root["event"]["state"].Value<string>("type") == "Peripheral"
                        status: root["event"]["state"].Value<uint>("status"),
                        serialNumber: root["event"]["state"].Value<string>("id"))
                    );
                    Debug.Log("Device added");
                }

            }
            else if (root.ContainsKey("id"))
            { // Frame

                // with the help of https://github.com/leapmotion/UnityModules/blob/develop/Assets/Plugins/LeapMotion/Core/Scripts/Encoding/VectorHand.cs#L109-L208
                // example: {"currentFrameRate":115.304153,"devices":[],"hands":[],"id":1184204,"pointables":[],"timestamp":22483904428}
                // example: {"currentFrameRate":115.309593,"devices":[],"hands":[{"armBasis":[[0.717785,-0.030000,-0.695619],[0.169255,0.976621,0.132530],[0.675380,-0.212865,0.706081]],"armWidth":61.510147,"confidence":1.000000,"direction":[-0.340442,0.189902,-0.920889],"elbow":[251.853210,146.535797,260.110901],"grabAngle":0.679801,"grabStrength":0.000000,"id":137,"palmNormal":[-0.171231,-0.975537,-0.137869],"palmPosition":[41.654118,212.352127,15.091011],"palmVelocity":[-340.236908,422.169128,295.968994],"palmWidth":87.056808,"pinchDistance":64.991867,"pinchStrength":0.000000,"timeVisible":0.789211,"type":"right","wrist":[77.401672,201.519211,77.729134]}],"id":1522442,"pointables":[{"bases":[[[0.458756,0.865038,-0.203108],[-0.534627,0.451291,0.714500],[0.709730,-0.219195,0.669505]],[[0.319145,0.899909,-0.297170],[-0.315734,0.396617,0.861979],[0.893565,-0.181269,0.410710]],[[0.320766,0.899643,-0.296229],[-0.491181,0.425414,0.760108],[0.809846,-0.098315,0.578346]],[[0.323047,0.899565,-0.293980],[-0.738312,0.433889,0.516368],[0.592061,0.050238,0.804326]]],"btipPosition":[-35.915592,208.656891,19.983206],"carpPosition":[39.855389,198.139160,69.972702],"dipPosition":[-26.188410,209.482269,33.197765],"direction":[-0.809846,0.098315,-0.578346],"extended":true,"handId":137,"id":1370,"length":47.572941,"mcpPosition":[39.855389,198.139160,69.972702],"pipPosition":[-0.966919,206.420395,51.209538],"timeVisible":0.789211,"tipPosition":[-35.915592,208.656891,19.983206],"type":0,"width":19.624594},{"bases":[[[0.854257,-0.077291,-0.514074],[0.096525,0.995272,0.010760],[0.510812,-0.058813,0.857679]],[[0.858700,-0.078364,-0.506451],[0.159135,0.980161,0.118156],[0.487145,-0.182055,0.854135]],[[0.846892,-0.078307,-0.525968],[-0.028282,0.981065,-0.191600],[0.531012,0.177140,0.828642]],[[0.831041,-0.090708,-0.548765],[-0.280250,0.783938,-0.553987],[0.480448,0.614177,0.626064]]],"btipPosition":[-24.203382,218.012527,-56.111340],"carpPosition":[48.259079,217.969177,63.433365],"dipPosition":[-18.464060,225.349350,-48.632526],"direction":[-0.531012,-0.177140,-0.828642],"extended":true,"handId":137,"id":1371,"length":53.528511,"mcpPosition":[12.303341,222.108978,3.061852],"pipPosition":[-6.788830,229.244080,-30.413387],"timeVisible":0.789211,"tipPosition":[-24.203382,218.012527,-56.111340],"type":1,"width":18.745413},{"bases":[[[0.886360,-0.253056,-0.387723],[0.250749,0.966344,-0.057478],[0.389219,-0.046276,0.919982]],[[0.916704,-0.261944,-0.301726],[0.293564,0.953804,0.063861],[0.271060,-0.147118,0.951253]],[[0.924302,-0.260595,-0.278847],[0.163796,0.930758,-0.326894],[0.344726,0.256475,0.902986]],[[0.935220,-0.245728,-0.254914],[-0.022796,0.676678,-0.735926],[0.353332,0.694064,0.627242]]],"btipPosition":[6.148673,212.387207,-78.976486],"carpPosition":[57.715096,218.622589,56.168468],"dipPosition":[10.808395,221.540482,-70.704453],"direction":[-0.344726,-0.256475,-0.902986],"extended":true,"handId":137,"id":1372,"length":61.250515,"mcpPosition":[31.733868,221.711578,-5.242323],"pipPosition":[19.767721,228.206192,-47.236118],"timeVisible":0.789211,"tipPosition":[6.148673,212.387207,-78.976486],"type":2,"width":18.410486},{"bases":[[[0.912534,-0.329230,-0.242671],[0.325714,0.943827,-0.055675],[0.247369,-0.028236,0.968510]],[[0.932287,-0.339867,-0.123822],[0.353321,0.928962,0.110429],[0.077495,-0.146700,0.986141]],[[0.935039,-0.339421,-0.102450],[0.308937,0.921775,-0.234282],[0.173956,0.187412,0.966755]],[[0.941260,-0.328588,-0.077842],[0.208594,0.747055,-0.631188],[0.265553,0.577874,0.771714]]],"btipPosition":[41.377724,210.549789,-83.114532],"carpPosition":[67.326782,215.225861,50.301086],"dipPosition":[44.882664,218.176956,-72.928940],"direction":[-0.173956,-0.187412,-0.966755],"extended":true,"handId":137,"id":1373,"length":59.237312,"mcpPosition":[52.501362,216.918091,-7.743916],"pipPosition":[49.314270,222.951355,-48.300476],"timeVisible":0.789211,"tipPosition":[41.377724,210.549789,-83.114532],"type":3,"width":17.518744},{"bases":[[[0.868586,-0.481677,-0.116388],[0.484248,0.874903,-0.006956],[0.105179,-0.050319,0.993179]],[[0.859129,-0.501360,0.102646],[0.475144,0.855954,0.203915],[-0.190095,-0.126418,0.973593]],[[0.857152,-0.501056,0.119300],[0.513309,0.850101,-0.117648],[-0.042468,0.162080,0.985863]],[[0.858229,-0.495020,0.135642],[0.486725,0.701032,-0.521203],[0.162916,0.513332,0.842584]]],"btipPosition":[76.470367,204.061935,-67.422142],"carpPosition":[77.340340,206.329819,47.375893],"dipPosition":[78.455353,210.316422,-57.156025],"direction":[0.042468,-0.162080,-0.985863],"extended":true,"handId":137,"id":1374,"length":46.460064,"mcpPosition":[71.505157,209.121429,-7.724407],"pipPosition":[77.690643,213.234924,-39.404060],"timeVisible":0.789211,"tipPosition":[76.470367,204.061935,-67.422142],"type":4,"width":15.561518}],"timestamp":25419627604}

                Quaternion q; // temporary quaternion value

                _currentFrame.Id = root.Value<long>("id");
                _currentFrame.Timestamp = root.Value<long>("timestamp");
                _currentFrame.CurrentFramesPerSecond = root.Value<float>("currentFrameRate");

                List<Finger> fingers = new List<Finger>();
                foreach (JObject jsonFinger in root["pointables"])
                {
                    /*
                     * {
                      "bases": [
                        [[0.458756, 0.865038, -0.203108], [-0.534627, 0.451291, 0.714500], [0.709730, -0.219195, 0.669505]],
                        [[0.319145, 0.899909, -0.297170], [-0.315734, 0.396617, 0.861979], [0.893565, -0.181269, 0.410710]],
                        [[0.320766, 0.899643, -0.296229], [-0.491181, 0.425414, 0.760108], [0.809846, -0.098315, 0.578346]],
                        [[0.323047, 0.899565, -0.293980], [-0.738312, 0.433889, 0.516368], [0.592061, 0.050238, 0.804326]]
                      ],

                      "btipPosition": [-35.915592, 208.656891, 19.983206],

                      "carpPosition": [39.855389, 198.139160, 69.972702],

                      "dipPosition": [-26.188410, 209.482269, 33.197765],

                      "direction": [-0.809846, 0.098315, -0.578346],
                      "extended": true,
                      "handId": 137,
                      "id": 1370,
                      "length": 47.572941,

                      "mcpPosition": [39.855389, 198.139160, 69.972702],
                      "pipPosition": [-0.966919, 206.420395, 51.209538],

                      "timeVisible": 0.789211,
                      "tipPosition": [-35.915592, 208.656891, 19.983206],
                      "type": 0,
                      "width": 19.624594
                    }

                     * CHECK ALSO https://github.com/leapmotion/leapjs/blob/master/leap-1.0.0.js
                     * "bases": the 3 basis vectors for each bone, in index order, wrist to tip, (array of vectors).
                     * "btipPosition": the position of the tip of the distal phalanx as an array of 3 floats.
                     * "carpPosition": the position of the base of metacarpal bone as an array of 3 floats.
                     * "dipPosition:" the position of the base of the distal phalanx as an array of 3 floats.
                     * "mcpPosition": a position vector as an array of 3 floating point numbers ??? The metacarpopophalangeal joint is located at the base of a finger between
      * the metacarpal bone and the first phalanx.
                     * "pipPosition": a position vector as an array of 3 floating point numbers ??? position of the proximal interphalangeal joint
                     */

                    /*
                     * https://developer-archive.leapmotion.com/documentation/python/devguide/Leap_Overview.html
                     * The bones from the wrist to the tip are identified as:
                     * - Metacarpal – the bone inside the hand connecting the finger to the wrist (except the thumb)
                     * - Proximal Phalanx – the bone at the base of the finger, connected to the palm
                     * - Intermediate Phalanx – the middle bone of the finger, between the tip and the base
                     * - Distal Phalanx – the terminal bone at the end of the finger
                     */

                    /*
                     * https://developer-archive.leapmotion.com/documentation/v2/javascript/api/Leap.Bone.html#Bone.basis
                     * Basis vectors specify the orientation of a bone.
                     * - basis[0] – the x-basis. Perpendicular to the longitudinal axis of the bone; exits the sides of the finger or arm.
                     * - basis[1] – the y-basis or up vector. Perpendicular to the longitudinal axis of the bone; exits the top and bottom of the finger or arm. More positive in the upward direction.
                     * - basis[2] – the z-basis. Aligned with the longitudinal axis of the bone. More positive toward the base of the finger or elbow of the arm.
                     *
                     * The bases provided for the right hand use the right-hand rule; those for the left hand use the left-hand rule.
                     * Thus, the positive direction of the x-basis is to the right for the right hand and to the left for the left hand.
                     * You can change from right-hand to left-hand rule by multiplying the basis vectors by -1.
                     * You can use the basis vectors for such purposes as measuring complex finger poses and skeletal animation.
                     * The matrix() function converts the basis and bone positions to a transformation matrix that can be used to
                     * update a 3D object representing the bone in a 3D scene.
                     *
                     * Note that converting the basis vectors directly into a quaternion representation is not mathematically valid.
                     * If you use quaternions, create them from the derived rotation matrix not directly from the bases.
                     */

                    const float fingerWidth = .01f;

                    // https://github.com/leapmotion/leapjs/blob/master/leap-1.0.0.js
                    // this.positions = [this.carpPosition, this.mcpPosition, this.pipPosition, this.dipPosition, this.tipPosition];
                    Vector metacarpalBasePosition = FromFloats(jsonFinger["carpPosition"].Values<float>());
                    Vector proximalPhalanxBasePosition = FromFloats(jsonFinger["mcpPosition"].Values<float>()); // TODO CHECK
                    Vector intermediatePhalanxBasePosition = FromFloats(jsonFinger["pipPosition"].Values<float>()); // TODO CHECK
                    Vector distalPhalanxBasePosition = FromFloats(jsonFinger["dipPosition"].Values<float>());
                    Vector distalPhalanxTipPosition = FromFloats(jsonFinger["btipPosition"].Values<float>());

                    JToken bases = jsonFinger["bases"];
                    Vector xBasis, yBasis, zBasis;

                    // Compute metacarpal rotation quaternion
                    xBasis = FromFloats(bases[0][0].Values<float>());
                    yBasis = FromFloats(bases[0][1].Values<float>());
                    zBasis = FromFloats(bases[0][2].Values<float>());
                    q = Quaternion.LookRotation(
                        new Vector3(zBasis.x, zBasis.y, zBasis.z),
                        new Vector3(yBasis.x, yBasis.y, yBasis.z)
                    );
                    LeapQuaternion metacarpalRotation = new LeapQuaternion(q.x, q.y, q.z, q.w); // FromMat3(bases[0]);

                    // Compute proximal rotation quaternion
                    xBasis = FromFloats(bases[1][0].Values<float>());
                    yBasis = FromFloats(bases[1][1].Values<float>());
                    zBasis = FromFloats(bases[1][2].Values<float>());
                    q = Quaternion.LookRotation(
                        new Vector3(zBasis.x, zBasis.y, zBasis.z),
                        new Vector3(yBasis.x, yBasis.y, yBasis.z)
                    );
                    LeapQuaternion proximalRotation = new LeapQuaternion(q.x, q.y, q.z, q.w); // FromMat3(bases[1]);

                    // Compute intermediate rotation quaternion
                    xBasis = FromFloats(bases[2][0].Values<float>());
                    yBasis = FromFloats(bases[2][1].Values<float>());
                    zBasis = FromFloats(bases[2][2].Values<float>());
                    q = Quaternion.LookRotation(
                        new Vector3(zBasis.x, zBasis.y, zBasis.z),
                        new Vector3(yBasis.x, yBasis.y, yBasis.z)
                    );
                    LeapQuaternion intermediateRotation = new LeapQuaternion(q.x, q.y, q.z, q.w); // FromMat3(bases[2]);

                    // Compute distal rotation quaternion
                    xBasis = FromFloats(bases[3][0].Values<float>());
                    yBasis = FromFloats(bases[3][1].Values<float>());
                    zBasis = FromFloats(bases[3][2].Values<float>());
                    q = Quaternion.LookRotation(
                        new Vector3(zBasis.x, zBasis.y, zBasis.z),
                        new Vector3(yBasis.x, yBasis.y, yBasis.z)
                    );
                    LeapQuaternion distalRotation = new LeapQuaternion(q.x, q.y, q.z, q.w); // FromMat3(bases[3]);

                    int fingerId = jsonFinger.Value<int>("id");
                    float metacarpalLength = (proximalPhalanxBasePosition - metacarpalBasePosition).Magnitude;

                    // bool isLeft = false; // TODO
                    // Vector3 metacarpalDirection = (proximalPhalanxBasePosition - metacarpalBasePosition).Normalized.ToVector3();
                    // q = Quaternion.LookRotation(metacarpalDirection,
                    //     Vector3.Cross(metacarpalDirection, fingerId == 0 ? (isLeft ? Vector3.down : Vector3.up) : Vector3.right));
                    // LeapQuaternion metacarpalRotation = new LeapQuaternion(q.x, q.y, q.z, q.w);
                    //
                    // Vector3 proximalDirection = (intermediatePhalanxBasePosition - proximalPhalanxBasePosition).Normalized.ToVector3();
                    // q = Quaternion.LookRotation(proximalDirection,
                    //     Vector3.Cross(proximalDirection, fingerId == 0 ? (isLeft ? Vector3.down : Vector3.up) : Vector3.right));
                    // LeapQuaternion proximalRotation = new LeapQuaternion(q.x, q.y, q.z, q.w);
                    //
                    // Vector3 intermediateDirection = (distalPhalanxBasePosition - intermediatePhalanxBasePosition).Normalized.ToVector3();
                    // q = Quaternion.LookRotation(intermediateDirection,
                    //     Vector3.Cross(intermediateDirection, fingerId == 0 ? (isLeft ? Vector3.down : Vector3.up) : Vector3.right));
                    // LeapQuaternion intermediateRotation = new LeapQuaternion(q.x, q.y, q.z, q.w);
                    //
                    // Vector3 distalDirection = (distalPhalanxTipPosition - distalPhalanxBasePosition).Normalized.ToVector3();
                    // q = Quaternion.LookRotation(distalDirection,
                    //     Vector3.Cross(distalDirection, fingerId == 0 ? (isLeft ? Vector3.down : Vector3.up) : Vector3.right));
                    // LeapQuaternion distalRotation = new LeapQuaternion(q.x, q.y, q.z, q.w);

                    Finger.FingerType fingerType = (Finger.FingerType)jsonFinger.Value<int>("type");

                    fingers.Add(new Finger(
                        frameId: _currentFrame.Timestamp,
                        handId: jsonFinger.Value<int>("handId"),
                        fingerId: fingerId,
                        timeVisible: jsonFinger.Value<float>("timeVisible"),
                        tipPosition: FromFloats(jsonFinger["tipPosition"].Values<float>()),
                        direction: FromFloats(jsonFinger["direction"].Values<float>()),
                        width: jsonFinger.Value<float>("width"),
                        length: jsonFinger.Value<float>("length"),
                        isExtended: jsonFinger.Value<bool>("extended"),
                        type: fingerType,
                        metacarpal: new Bone(
                            prevJoint: metacarpalBasePosition,
                            nextJoint: proximalPhalanxBasePosition,
                            center: (metacarpalBasePosition + proximalPhalanxBasePosition) / 2f,
                            direction: (proximalPhalanxBasePosition - metacarpalBasePosition).Normalized,
                            length: metacarpalLength,
                            width: fingerWidth,
                            type: Bone.BoneType.TYPE_METACARPAL,
                            rotation: fingerType == Finger.FingerType.TYPE_THUMB ? LeapQuaternion.Identity : metacarpalRotation), // for the thumb exception
                        proximal: new Bone(
                            prevJoint: proximalPhalanxBasePosition,
                            nextJoint: intermediatePhalanxBasePosition,
                            center: (proximalPhalanxBasePosition + intermediatePhalanxBasePosition) / 2f,
                            direction: (intermediatePhalanxBasePosition - proximalPhalanxBasePosition).Normalized,
                            length: (intermediatePhalanxBasePosition - proximalPhalanxBasePosition).Magnitude,
                            width: fingerWidth,
                            type: Bone.BoneType.TYPE_PROXIMAL,
                            rotation: proximalRotation),
                        intermediate: new Bone(
                            prevJoint: intermediatePhalanxBasePosition,
                            nextJoint: distalPhalanxBasePosition,
                            center: (intermediatePhalanxBasePosition + distalPhalanxBasePosition) / 2f,
                            direction: (distalPhalanxBasePosition - intermediatePhalanxBasePosition).Normalized,
                            length: (distalPhalanxBasePosition - intermediatePhalanxBasePosition).Magnitude,
                            width: fingerWidth,
                            type: Bone.BoneType.TYPE_INTERMEDIATE,
                            rotation: intermediateRotation),
                        distal: new Bone(
                            prevJoint: distalPhalanxBasePosition,
                            nextJoint: distalPhalanxTipPosition,
                            center: (distalPhalanxBasePosition + distalPhalanxTipPosition) / 2f,
                            direction: (distalPhalanxTipPosition - distalPhalanxBasePosition).Normalized,
                            length: (distalPhalanxTipPosition - distalPhalanxBasePosition).Magnitude,
                            width: fingerWidth,
                            type: Bone.BoneType.TYPE_DISTAL,
                            rotation: distalRotation)
                    ));
                }

                List<Hand> hands = new List<Hand>();
                foreach (JObject jsonHand in root["hands"])
                {
                    /*
                     * {
                      "armBasis": [[0.717785, -0.030000, -0.695619], [0.169255, 0.976621, 0.132530], [0.675380, -0.212865, 0.706081]],
                      "armWidth": 61.510147,
                      "confidence": 1.000000,
                      "direction": [-0.340442, 0.189902, -0.920889],
                      "elbow": [251.853210, 146.535797, 260.110901],
                      "grabAngle": 0.679801,
                      "grabStrength": 0.000000,
                      "id": 137,
                      "palmNormal": [-0.171231, -0.975537, -0.137869],
                      "palmPosition": [41.654118, 212.352127, 15.091011],
                      "palmVelocity": [-340.236908, 422.169128, 295.968994],
                      "palmWidth": 87.056808,
                      "pinchDistance": 64.991867,
                      "pinchStrength": 0.000000,
                      "timeVisible": 0.789211,
                      "type": "right",
                      "wrist": [77.401672, 201.519211, 77.729134]
                    }
                     *
                     * Hand.basis is computed from the palmNormal and direction vectors
                     * Basis vectors fo the arm property specify the orientation of a arm:
                     * - arm.basis[0] – the x-basis. Perpendicular to the longitudinal axis of the arm; exits laterally from the sides of the wrist.
                     * - arm.basis[1] – the y-basis or up vector. Perpendicular to the longitudinal axis of the arm; exits the top and bottom of the arm. More positive in the upward direction.
                     * - arm.basis[2] – the z-basis. Aligned with the longitudinal axis of the arm. More positive toward the elbow.
                     *
                     * The bases provided for the right arm use the right-hand rule; those for the left arm use the left-hand rule.
                     * Thus, the positive direction of the x-basis is to the right for the right arm and to the left for the left arm.
                     * You can change from right-hand to left-hand rule by multiplying the basis vectors by -1.
                     */

                    // Compute arm rotation quaternion
                    Vector xBasis = FromFloats(jsonHand["armBasis"][0].Values<float>());
                    Vector yBasis = FromFloats(jsonHand["armBasis"][1].Values<float>());
                    Vector zBasis = FromFloats(jsonHand["armBasis"][2].Values<float>());
                    q = Quaternion.LookRotation(
                        new Vector3(zBasis.x, zBasis.y, zBasis.z),
                        new Vector3(yBasis.x, yBasis.y, yBasis.z)
                    );
                    LeapQuaternion armRotation = new LeapQuaternion(q.x, q.y, q.z, q.w);

                    // Compute palm rotation quaternion
                    Vector palmDirection = FromFloats(jsonHand["direction"].Values<float>());
                    Vector palmNormal = FromFloats(jsonHand["palmNormal"].Values<float>());
                    q = Quaternion.LookRotation(
                    -new Vector3(palmDirection.x, palmDirection.y, palmDirection.z),
                    -new Vector3(palmNormal.x, palmNormal.y, palmNormal.z)
                    );
                    LeapQuaternion palmRotation = new LeapQuaternion(q.x, q.y, q.z, q.w);

                    int handId = jsonHand.Value<int>("id");
                    Vector elbowPosition = FromFloats(jsonHand["elbow"].Values<float>());
                    Vector wristPosition = FromFloats(jsonHand["wrist"].Values<float>());
                    Hand hand = new Hand(
                        frameID: _currentFrame.Timestamp,
                        id: handId,
                        confidence: jsonHand.Value<float>("confidence"),
                        grabStrength: jsonHand.Value<float>("grabStrength"),
                        grabAngle: jsonHand.Value<float>("grabAngle"),
                        pinchStrength: jsonHand.Value<float>("pinchStrength"),
                        pinchDistance: jsonHand.Value<float>("pinchDistance"),
                        palmWidth: jsonHand.Value<float>("palmWidth"),
                        isLeft: jsonHand.Value<string>("type").Equals("left"),
                        timeVisible: jsonHand.Value<float>("timeVisible"),
                        arm: new Arm(
                            elbow: elbowPosition,
                            wrist: wristPosition,
                            center: (elbowPosition + wristPosition) / 2f,
                            direction: (wristPosition - elbowPosition).Normalized/*-palmDirection*/,
                            length: (wristPosition - elbowPosition).Magnitude,
                            width: jsonHand.Value<float>("armWidth"),
                            rotation: armRotation/*palmRotation*/ // TODO CHECK
                        ),
                        fingers: fingers.Where(f => f.HandId == handId).OrderBy(f => f.Type).ToList(), // need to order by type because leap motion expect it (as detailed in doc: https://developer-archive.leapmotion.com/documentation/v2/csharp/api/Leap.Hand.html#csharpclass_leap_1_1_hand_1a34356976500331d2a1998cb6ad857dae)
                        palmPosition: FromFloats(jsonHand["palmPosition"].Values<float>()),
                        stabilizedPalmPosition: FromFloats(jsonHand["palmPosition"].Values<float>()), // TODO
                        palmVelocity: FromFloats(jsonHand["palmVelocity"].Values<float>()),
                        palmNormal: FromFloats(jsonHand["palmNormal"].Values<float>()),
                        palmOrientation: palmRotation, // TODO
                        direction: FromFloats(jsonHand["direction"].Values<float>()),
                        wristPosition: wristPosition
                    );
                    hands.Add(hand);
                }

                _currentFrame.Hands = hands;

                Frame currentFrame = new Frame().CopyFrom(_currentFrame);
                _frames.Put(currentFrame);
                FrameReady?.Invoke(this, new FrameEventArgs(currentFrame));
            }
        }

        Vector FromFloats(IEnumerable<float> floats)
        {
            float[] array = floats.ToArray();
            return new Vector(array[0], array[1], array[2]);
        }

        // http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/
        // LeapQuaternion QuaternionFromBasis(JToken basis) {
        //     float m00 = basis[0].Value<float>(0);
        //     float m01 = basis[0].Value<float>(1);
        //     float m02 = basis[0].Value<float>(2);
        //     float m10 = basis[1].Value<float>(0);
        //     float m20 = basis[2].Value<float>(0);
        //     float m11 = basis[1].Value<float>(1);
        //     float m12 = basis[1].Value<float>(2);
        //     float m21 = basis[2].Value<float>(1);
        //     float m22 = basis[2].Value<float>(2);
        //     
        //     float x, y, z, w;
        //     w = Mathf.Sqrt( Mathf.Max( 0f, 1f + m00 + m11 + m22 ) ) / 2f;
        //     x = Mathf.Sqrt( Mathf.Max( 0f, 1f + m00 - m11 - m22 ) ) / 2f;
        //     y = Mathf.Sqrt( Mathf.Max( 0f, 1f - m00 + m11 - m22 ) ) / 2f;
        //     z = Mathf.Sqrt( Mathf.Max( 0f, 1f - m00 - m11 + m22 ) ) / 2f;
        //
        //     x = Mathf.Sign(x) == Mathf.Sign(m21 - m12) ? x : -x;
        //     y = Mathf.Sign(x) == Mathf.Sign(m02 - m20) ? y : -y;
        //     z = Mathf.Sign(x) == Mathf.Sign(m10 - m01) ? z : -z;
        //
        //     return new LeapQuaternion(x,y,z,w);
        // }

        // That's not what we want because we have a base matrix and not a rotation one...
        /*
         * Creates a quaternion from the given 3x3 rotation matrix.
         *
         * NOTE: The resultant quaternion is not normalized, so you should be sure
         * to renormalize the quaternion yourself where necessary.
         *
         * @param {quat} out the receiving quaternion
         * @param {mat3} m rotation matrix
         * @returns {quat} out
         * @function
        */
        // LeapQuaternion FromMat3(JToken basis) {
        //     float[] qt = new float[4];
        //     
        //     float m00 = basis[0].Value<float>(0); // 0
        //     float m01 = basis[0].Value<float>(1); // 1
        //     float m02 = basis[0].Value<float>(2); // 2
        //     float m10 = basis[1].Value<float>(0); // 3
        //     float m11 = basis[1].Value<float>(1); // 4
        //     float m12 = basis[1].Value<float>(2); // 5
        //     float m20 = basis[2].Value<float>(0); // 6
        //     float m21 = basis[2].Value<float>(1); // 7
        //     float m22 = basis[2].Value<float>(2); // 8
        //     float[] m = { m00, m01, m02, m10, m11, m12, m20, m21, m22 };
        //     
        //     // Algorithm in Ken Shoemake's article in 1987 SIGGRAPH course notes
        //     // article "Quaternion Calculus and Fast Animation".
        //     float fTrace = m[0] + m[4] + m[8];
        //     float fRoot;
        //
        //     if (fTrace > 0f) {
        //         // |w| > 1/2, may as well choose w > 1/2
        //         fRoot = Mathf.Sqrt(fTrace + 1f); // 2w
        //
        //         qt[3] = .5f * fRoot;
        //         fRoot = .5f / fRoot; // 1/(4w)
        //
        //         qt[0] = (m[5] - m[7]) * fRoot;
        //         qt[1] = (m[6] - m[2]) * fRoot;
        //         qt[2] = (m[1] - m[3]) * fRoot;
        //     } else {
        //         // |w| <= 1/2
        //         var i = 0;
        //         if (m[4] > m[0]) i = 1;
        //         if (m[8] > m[i * 3 + i]) i = 2;
        //         var j = (i + 1) % 3;
        //         var k = (i + 2) % 3;
        //         fRoot = Mathf.Sqrt(m[i * 3 + i] - m[j * 3 + j] - m[k * 3 + k] + 1f);
        //         qt[i] = .5f * fRoot;
        //         fRoot = .5f / fRoot;
        //         qt[3] = (m[j * 3 + k] - m[k * 3 + j]) * fRoot;
        //         qt[j] = (m[j * 3 + i] + m[i * 3 + j]) * fRoot;
        //         qt[k] = (m[k * 3 + i] + m[i * 3 + k]) * fRoot;
        //     }
        //
        //     return new LeapQuaternion(qt[0], qt[1], qt[2], qt[3]);
        // }

        void OnOpen(object sender, EventArgs e)
        {
            Debug.Log($"OnOpen {sender} {e}");
            Connect?.Invoke(this, new ConnectionEventArgs());
        }

        public new void Dispose()
        {
            Debug.Log("Dispose Remote Controller");
            _webSocket?.Close();
        }

        public Frame Frame(int history = 0)
        {
            Frame frame = new Frame();

            // don't know if we really need to copying it, but that's what LeapMotion team did in their Controller class
            //frame.CopyFrom(_frames.History(history));
            Frame(frame, history);

            return frame;
        }

        public Frame GetTransformedFrame(LeapTransform trs, int history = 0)
        {
            return Frame(history).Transform(trs);
        }

        public Frame GetInterpolatedFrame(long time)
        {
            Frame frame = new Frame();
            GetInterpolatedFrame(frame, time);
            return frame;
        }

        public void SetPolicy(Controller.PolicyFlag policy)
        {
            if (IsPolicySet(policy)) return;
            _policy |= policy;
            UpdatePolicy(policy, true);
        }

        public void ClearPolicy(Controller.PolicyFlag policy)
        {
            if (!IsPolicySet(policy)) return;
            _policy &= ~policy;
            UpdatePolicy(policy, false);
        }

        public bool IsPolicySet(Controller.PolicyFlag policy)
        {
            return _policy.HasFlag(policy);
        }

        /// <summary>
        /// Send to the websocket server a message to update the policy
        /// The policy can be:
        /// - background: Specifies whether the client wants to receive frames when it is not the focused application
        /// - focused: Specifies whether the application is active. Setting focused to true, stops other WebSocket clients
        ///             from getting data. Setting focused to false stops your WebSocket client from getting data
        ///             (and potentially unwanted input)
        /// - enableGestures: Enables or disables gesture recognition data
        /// - optimizeHMD: Specifies whether the client application expects the Leap Motion hardware to be attached to a head-mounted display
        ///
        /// example: {"optimizeHMD": true}
        /// </summary>
        void UpdatePolicy(Controller.PolicyFlag policyFlag, bool enable)
        {
            Debug.Log($"Update Policy {policyFlag} {enable}");

            string policyName;
            switch (policyFlag)
            {
                case Controller.PolicyFlag.POLICY_DEFAULT:
                    throw new NotImplementedException();
                case Controller.PolicyFlag.POLICY_BACKGROUND_FRAMES:
                    policyName = "background";
                    break;
                case Controller.PolicyFlag.POLICY_IMAGES:
                    throw new NotSupportedException();
                case Controller.PolicyFlag.POLICY_OPTIMIZE_HMD:
                    policyName = "optimizeHMD";
                    break;
                case Controller.PolicyFlag.POLICY_ALLOW_PAUSE_RESUME:
                    throw new NotSupportedException();
                case Controller.PolicyFlag.POLICY_MAP_POINTS:
                    throw new NotSupportedException();
                default:
                    throw new ArgumentOutOfRangeException(nameof(policyFlag), policyFlag, null);
            }
            _webSocket.Send(string.Format("{\"{0}\":{1}}", policyName, enable ? "true" : "false"));
        }

        public long Now()
        {
            return DateTime.Now.ToBinary(); // TODO check if we need the remote time or the time from the current machine...
        }

        public bool IsConnected => IsServiceConnected && Devices.Count > 0;

        public Config Config { get; }

        public DeviceList Devices { get; private set; } = new DeviceList();

        public event EventHandler<ConnectionEventArgs> Connect;
        public event EventHandler<ConnectionLostEventArgs> Disconnect;
        public event EventHandler<FrameEventArgs> FrameReady;
        public event EventHandler<DeviceEventArgs> Device;
        public event EventHandler<DeviceEventArgs> DeviceLost;
        public event EventHandler<DeviceFailureEventArgs> DeviceFailure;
        public event EventHandler<LogEventArgs> LogMessage;
        public event EventHandler<PolicyEventArgs> PolicyChange;
        public event EventHandler<ConfigChangeEventArgs> ConfigChange;
        public event EventHandler<DistortionEventArgs> DistortionChange;
        public event EventHandler<ImageEventArgs> ImageReady;
        public event EventHandler<PointMappingChangeEventArgs> PointMappingChange;
        public event EventHandler<HeadPoseEventArgs> HeadPoseChange;

        public long FrameTimestamp(int history = 0)
        {
            return Frame(history).Timestamp;
        }

        public void GetInterpolatedLeftRightTransform(long time, long sourceTime, int leftId, int rightId,
            out LeapTransform leftTransform, out LeapTransform rightTransform)
        {
            // TODO interpolate

            leftTransform = LeapTransform.Identity;
            rightTransform = LeapTransform.Identity;

            Frame frame = Frame();
            foreach (Hand hand in frame.Hands)
            {
                if (hand.IsLeft)
                {
                    leftTransform = hand.Basis;
                }
                else if (hand.IsRight)
                {
                    rightTransform = hand.Basis;
                }
                else
                {
                    throw new ArgumentException(); // not the appropriate one, but I don't remember the right exception...
                }
            }
        }

        public void GetInterpolatedFrameFromTime(Frame toFill, long time, long sourceTime)
        {
            // TODO interpolate
            Frame(toFill);
        }

        public void GetInterpolatedFrame(Frame toFill, long time)
        {
            // TODO interpolate
            Frame(toFill);
        }

        public void Frame(Frame toFill, int history = 0)
        {
            Frame frame = _frames.History(history);
            if (frame != null) toFill.CopyFrom(frame);
        }

        public void StartConnection()
        {
            _webSocket.Connect();
        }

        public void StopConnection()
        {
            _webSocket.Close();
        }

        public event Action<EndProfilingBlockArgs> EndProfilingBlock;
        public event Action<BeginProfilingBlockArgs> BeginProfilingBlock;
        public event Action<EndProfilingForThreadArgs> EndProfilingForThread;
        public event Action<BeginProfilingForThreadArgs> BeginProfilingForThread;

        public bool IsServiceConnected => _webSocket != null && _webSocket.ReadyState == WebSocketState.Open;
    }