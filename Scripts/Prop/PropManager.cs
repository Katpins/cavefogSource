using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System;

namespace caveFog
{
    public class PropManager : MonoBehaviour
    {
        public PropPlacement[] _chunkPropPositions;
        public float3[] _mainPath;
        public float3[] _roomPosition;
        public bool _disablePropGen = false;

        [SerializeField]
        private PropCollection _propCollection;

        [SerializeField]
        private Prop _testProp;

        [Range(0f, 10f)]
        public float _propPlaceRadius = 5f;
        private LayerMask _layerMask;

        private Coroutine _setUpCoroutine;
        private List<float3> _flockPoints = new List<float3>();

        private List<PropPlacement> _currentPlacements = new List<PropPlacement>();
        private List<PropObject> _currentProps = new List<PropObject>();

        private float2 _maxCoord;

        //method for starting prop placement
        public void SetUpProps(Chunk _chunk)
        {
            if (_disablePropGen) return;
            _layerMask = LayerMask.GetMask("CaveMesh");
            _mainPath = _chunk._wormPath;
            _roomPosition = _chunk._roomPositions;

            //create max positions in the chunk
            _maxCoord = math.float2(_chunk._settings._widthWU / 2f, _chunk._settings._lengthWU / 2f);

            for (int i = 0; i < _propCollection.Length; i++)
            {
                ///important to clean before next type
                _currentPlacements.Clear();

                if (_propCollection[i].RoomProp)
                    GetPropPositions(_propCollection[i], _chunk, _roomPosition);

                if (_propCollection[i].MainPathProp)
                    GetPropPositions(_propCollection[i], _chunk, _mainPath);

                if (_propCollection[i].CeilingProp)
                    GetPropPositions(_propCollection[i], _chunk, _chunk._ceilingPath);

                if (_propCollection[i].PitsProp)
                    GetPropPositions(_propCollection[i], _chunk, _chunk._pitPath);

                _propCollection[i].PossiblePositions = _currentPlacements.ToArray();
                _propCollection[i].PlaceProps(this.transform, _chunk.RandomGenerator, _currentProps);
            }

        }

        // go through current chunk props and disable them by returning them to the pool
        public void RemoveProps()
        {
            for (int i = 0; i < _currentProps.Count; i++)
            {
                _currentProps[i].Disable();
            }
            _currentProps.Clear();
        }

        //get all posiible positions in chunk
        private void GetPropPositions(PropType _propType, Chunk in_chunk, float3[] _positions)
        {
            //List<PropPlacement> _placements = new List<PropPlacement>();

            RaycastHit _hit;
            float3 _stepDirection;
            int _id;
            int _chance;

            //cycle through path positions and cast rays around to find posible points
            //to avoid to many casts we do it only every second step
            for (int i = 1; i < _positions.Length - _propType.SpawnStep; i += _propType.SpawnStep)
            {
                float3 origin;
                float angle;
                float3 point;
                RaycastHit _flockHit;

                _stepDirection = math.normalize(_positions[i + _propType.SpawnStep] - _positions[i]);

                _chance = _propType.SpawnChance == 100 ? 101 : in_chunk.RandomGenerator.Next(0, 100);

                if (_chance > _propType.SpawnChance) continue;

                //shoot 5 rays in random directions
                for (int j = 0; j < _propType.RaysAmount; j++)
                {
                    if (Physics.Raycast(_positions[i], RandomOnUnitSphere(in_chunk.RandomGenerator, _propType, _stepDirection), out _hit, 10f, _layerMask.value))
                    {


                        if (!CheckPlacementNormal(_propType, _hit.normal)) continue;

                        //check if it doesnt lie on the chunk edge
                        if (!CheckIfInsideChunk(_hit.point)) continue;

                        //check if to close to previous points
                        if (_currentPlacements.Contains(_currentPlacements.FindLast(x => math.length((float3)_hit.point - x.Position) < _propType.PlacementRadius))) continue;

                        //if prop can flock then we make extra raycasts from hit point
                        if (_propType.Flocking)
                        {
                            _flockPoints.Clear();
                            origin = _hit.normal * _propType.FlockingSettings.RaycastOriginDistance + _hit.point;

                            for (int f = 0; f < _propType.FlockingSettings.MaxPointAmount; f++)
                            {
                                //get the point

                                angle = UnityEngine.Random.Range(0f, math.PI * 2f);
                                point = math.float3(math.cos(angle), 0f, math.sin(angle)) * math.pow(UnityEngine.Random.Range(0f, 1), _propType.FlockingSettings.PlacementBias) * _propType.FlockingSettings.SpreadRadius;

                                point = Quaternion.FromToRotation(Vector3.up, _hit.normal) * point;
                                point += (float3)_hit.point;

                                //if (_flockPoints.Contains(_flockPoints.FindLast(x => math.length(point - x) < _propType.FlockingSettings.MinPlacementDistance))) continue;


                                if (Physics.Raycast(origin,
                                    math.normalize(point - origin),
                                    out _flockHit, 10f,
                                    _layerMask.value))
                                {
                                    _flockPoints.Add(point);

                                    if (!CheckPlacementNormal(_propType, _flockHit.normal)) continue;

                                    //check if to close to previous points
                                    if (_currentPlacements.Contains(_currentPlacements.FindLast(x => math.length((float3)_flockHit.point - x.Position) < _propType.FlockingSettings.MinPlacementDistance))) continue;

                                    //roll on which prop to spawn
                                    _id = _propType.GetRandomPropID(in_chunk.RandomGenerator);

                                    //Check if prop fits
                                    if (_propType[_id].CheckOverlap(_flockHit.point)) continue;

                                    //save position
                                    _currentPlacements.Add(new PropPlacement(_flockHit.point, _flockHit.normal, _id));

                                }

                            }

                            continue;
                        }

                        //roll on which prop to spawn
                        _id = _propType.GetRandomPropID(in_chunk.RandomGenerator);

                        //Check if prop fits
                        if (_propType[_id].CheckOverlap(_hit.point)) continue;

                        //save position
                        _currentPlacements.Add(new PropPlacement(_hit.point, _hit.normal, _id));


                    }

                }
            }

            // return _placements.ToArray();
        }


        //taking all placements and instatiationg props for them
        // private void PlaceProps(Chunk in_chunk, PropType in_type)
        // {
        //     for (int i = 0; i < _propCollection.Length; i++)
        //     {
        //         _propCollection[i].PlaceProps(this.transform, in_chunk.RandomGenerator);
        //         //_testProp.PlaceProp(_chunkPropPositions[i], this.transform);
        //     }

        // }

        private bool CheckPlacementNormal(PropType _type, float3 _normal)
        {

            return _normal.y <= _type.MaxNormalY && _normal.y >= _type.MinNormalY;
        }

        //check if position isnt close to the chuink edge to avoid flying props
        private bool CheckIfInsideChunk(Vector3 in_position)
        {
            float2 _relativePos = math.float2(in_position.x - transform.position.x, in_position.z - transform.position.z);

            _relativePos = math.abs(_relativePos);

            return _maxCoord.x - _relativePos.x > 0.05f && _maxCoord.y - _relativePos.y > 0.05f;
        }

        public float3 RandomOnUnitSphere(System.Random _seedGen, PropType _type, float3 _nextStepDir)
        {
            float3 _dir = math.float3(_seedGen.Next(-1000, 1000), _seedGen.Next((int)(1000f * _type.MinY), (int)(1000f * _type.MaxY)), _seedGen.Next(-1000, 1000)) / 1000f;

            if (_type.DeadZone)
            {
                //check normal coordinates
                if (math.abs(_dir.z) < _type.DeadZoneSizeZ)
                {
                    _dir.z = math.sign(_dir.z) * _type.DeadZoneSizeZ;
                }

                if (math.abs(_dir.x) < _type.DeadZoneSizeX)
                {
                    _dir.x = math.sign(_dir.x) * _type.DeadZoneSizeX;
                }


                float _angle = math.asin(_nextStepDir.z);

                _dir = rotateXZCoordinates(_dir, math.float3(0, 0, 0), _angle);

            }
            _dir = (1f / math.length(_dir)) * _dir;

            return _dir;
        }
        private float3 rotateXZCoordinates(float3 _coord, float3 _center, float _angle)
        {
            float3 newCoord = _coord;

            newCoord.x = _center.x + (_coord.x - _center.x) * math.cos(_angle) + (_coord.z - _center.z) * math.sin(_angle);
            newCoord.z = _center.z - (_coord.x - _center.x) * math.sin(_angle) + (_coord.z - _center.z) * math.cos(_angle);

            return newCoord;
        }
        private void OnDrawGizmos()
        {
            if (_chunkPropPositions == null) return;

            Gizmos.color = Color.green;

            for (int i = 0; i < _chunkPropPositions.Length; i++)
            {
                Gizmos.DrawCube(_chunkPropPositions[i].Position, Vector3.one * 0.2f);
            }
        }




    }

    public struct PropPlacement
    {
        private float3 _position;
        public float3 Position { get => _position; }

        private float3 _normal;
        public float3 Normal { get => _normal; }

        private int _propID;
        public int PropID { get => _propID; }

        public PropPlacement(float3 in_position, float3 in_normal, int in_propId)
        {
            _position = in_position;
            _normal = in_normal;
            _propID = in_propId;
        }
    }

}
