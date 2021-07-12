using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace caveFog
{
    public enum Side { Left, Right, Up, Down }



    public class Connection
    {
        public Vector3 _point;
        public Side _connectedSide;



        //public Chunk _connectedChunk = null;

        //public int _neighbourIndex = -1;

    }

    //[System.Serializable]
    public class Chunk
    {
        public ChunkSettings _settings = default;

        [SerializeField]
        public ChunkObject _chunkObject = null;

        [SerializeField]
        public Vector3 _centerPos = Vector3.zero;

        [SerializeField]
        public Vector3 _startPos = Vector3.zero;

        [SerializeField]
        public float[,,] _voxelMap = new float[1, 1, 1];

        //public float[] VoxelMap1D => Generate1DVoxelMap();
        public float[] _VoxelMap1D;

        //main path
        [SerializeField]
        public float3[] _wormPath;

        //ceiling path
        [SerializeField]
        public float3[] _ceilingPath;

        //pit path
        [SerializeField]
        public float3[] _pitPath;

        [SerializeField]
        public float3[] _roomPositions;

        public int _seed;
        public List<Chunk> _connectedChunks;

        [SerializeField]
        public List<Connection> _connections;

        //special connection from which all path calculations start
        [SerializeField]
        public Connection _entrance;

        public bool _initialised = false;

        public bool _visited = false;

        public bool _dying = false;

        private int[] _edgeVerticies;

        public int[] EdgeVerticies { get => _edgeVerticies; set => _edgeVerticies = value; }

        private System.Random seedGen;

        public System.Random RandomGenerator { get => seedGen; }


        public Chunk(Vector3 in_centerPos, ChunkSettings in_settings, int in_seed)
        {
            _seed = in_seed;

            seedGen = new System.Random(in_seed);

            _centerPos = in_centerPos;
            _settings = in_settings;
            _connections = new List<Connection>();
            _connectedChunks = new List<Chunk>();

            _voxelMap = new float[_settings._widthVoxels, _settings._heightVoxels, _settings._lengthVoxels];

            _startPos = _centerPos - new Vector3(_settings._widthWU, _settings._heightWU, _settings._lengthWU) / 2;

            CreateEntrance();
        }


        //WIP creating chunk Based on existing connection, turning it into entrance
        public Chunk(Chunk _parent, ChunkSettings in_settings, Connection in_entrance, int in_seed)
        {
            _seed = in_seed;

            seedGen = new System.Random(in_seed);

            _settings = in_settings;
            _connections = new List<Connection>();
            _connectedChunks = new List<Chunk>();
            _voxelMap = new float[_settings._widthVoxels, _settings._heightVoxels, _settings._lengthVoxels];

            SetUpAsNeighbour(_parent, in_entrance);

            _startPos = _centerPos - new Vector3(_settings._widthWU, _settings._heightWU, _settings._lengthWU) / 2;

        }

        private float[] Generate1DVoxelMap()
        {
            float[] _1dMap = new float[_settings._widthVoxels * _settings._heightVoxels * _settings._lengthVoxels];

            for (int x = 0; x < _settings._widthVoxels; x++)
            {
                for (int y = 0; y < _settings._heightVoxels; y++)
                {
                    for (int z = 0; z < _settings._lengthVoxels; z++)
                    {
                        _1dMap[x + y * _settings._widthVoxels + z * _settings._widthVoxels * _settings._heightVoxels] = _voxelMap[x, y, z];
                    }
                }
            }

            return _1dMap;

        }

        //remove all existing connections
        public void ClearConnections()
        {
            if (_connections != null) _connections.Clear();
            else Debug.LogError("Connections list doesnt exist... again");
        }

        public void CreateEntrance()
        {
            if (_entrance != null) Debug.LogWarning("Changing entrance");
            _entrance = CreateConnection(true);

            //adding entrance to connection so its easier to check all connected  chunks
            _connections.Add(_entrance);
        }

        public void AddConnection()
        {
            if (_connections == null)
            {
                _connections = new List<Connection>();
            }

            if (_connections.Count == 3)
            {
                Debug.LogError("Maximum connection amount reached!");
                return;
            }

            _connections.Add(CreateConnection());


        }

        //creates connected entrance and calculates center accordingly
        private void SetUpAsNeighbour(Chunk in_prevChunk, Connection in_connection)
        {
            Connection _newConnection = new Connection();

            Vector3 _offsetDirection = Vector3.zero;

            switch (in_connection._connectedSide)
            {
                case Side.Up:
                    _newConnection._connectedSide = Side.Down;
                    _offsetDirection = Vector3.forward * _settings._lengthWU;
                    break;

                case Side.Down:
                    _newConnection._connectedSide = Side.Up;
                    _offsetDirection = Vector3.back * _settings._lengthWU;
                    break;

                case Side.Left:
                    _newConnection._connectedSide = Side.Right;
                    _offsetDirection = Vector3.left * _settings._widthWU;
                    break;

                case Side.Right:
                    _newConnection._connectedSide = Side.Left;
                    _offsetDirection = Vector3.right * _settings._widthWU;
                    break;

                default:
                    Debug.LogError("Oh no...");
                    break;

            }

            _newConnection._point = in_connection._point;
            //_newConnection._connectedChunk = in_prevChunk;

            _centerPos = in_prevChunk._centerPos + _offsetDirection;
            //in_connection._connectedChunk = this;

            in_prevChunk._connectedChunks.Add(this);
            _connectedChunks.Add(in_prevChunk);

            // in_connection._neighbourIndex = in_prevChunk._connectedChunks.Count - 1;
            // _newConnection._neighbourIndex = _connectedChunks.Count - 1;

            _entrance = _newConnection;

            _connections.Add(_entrance);
        }

        //creating the connection or end Point for chunk /// SPECIFY CHUNK IN THE FUTURE
        private Connection CreateConnection(bool _isEntrance = false)
        {

            Side _connectionSide;
            //randomly pick side for connection until we get one we dont have
            do
            {
                _connectionSide = (Side)seedGen.Next(0, 4);

                if (_isEntrance) break;

            } while (CheckIfConnectionExists(_connectionSide));

            Connection _newConnection = new Connection();

            do
            {
                _newConnection._point = CreateConnectionPoint(_connectionSide);
                if (_isEntrance) break;

            } while (((_newConnection._point - _entrance._point).sqrMagnitude < _settings._widthWU * _settings._widthWU / 2));

            _newConnection._connectedSide = _connectionSide;

            return _newConnection;

        }

        //check if this chunk side already has the connection
        private bool CheckIfConnectionExists(Side _newSide)
        {
            if (_newSide == _entrance._connectedSide) return true;

            for (int i = 0; i < _connections.Count; i++)
            {
                if (_newSide == _connections[i]._connectedSide) return true;
            }

            return false;
        }

        //reset all voxel values
        public void ClearVoxels()
        {
            if (_voxelMap == null)
            {
                Debug.LogError("Voxel Data doesnt exist!");
                return;
            }

            for (int x = 0; x < _settings._widthVoxels; x++)
            {
                for (int y = 0; y < _settings._heightVoxels; y++)
                {
                    for (int z = 0; z < _settings._lengthVoxels; z++)
                    {
                        _voxelMap[x, y, z] = 0;
                    }
                }
            }
        }

        //choose random point on chosen side
        private Vector3 CreateConnectionPoint(Side _side)
        {
            float x = 0;
            float z = 0;

            switch (_side)
            {
                case Side.Up:
                    x = seedGen.Next(-(_settings._widthWU - 4), (_settings._widthWU - 4));
                    z = _settings._lengthWU;
                    break;

                case Side.Down:
                    x = seedGen.Next(-(_settings._widthWU - 4), (_settings._widthWU - 4));
                    z = -_settings._lengthWU;
                    break;

                case Side.Left:
                    z = seedGen.Next(-(_settings._lengthWU - 4), (_settings._lengthWU - 4));
                    x = -_settings._widthWU;
                    break;

                case Side.Right:
                    z = seedGen.Next(-(_settings._lengthWU - 4), (_settings._lengthWU - 4));
                    x = _settings._widthWU;
                    break;

                default:
                    Debug.LogError("Oh no...");
                    break;
            }

            return new Vector3(x, 0, z) / 2f + _centerPos;
        }

        public Vector3 GetConnectionNormal(Connection in_connection)
        {

            switch (in_connection._connectedSide)
            {
                case Side.Up:
                    return Vector3.back;


                case Side.Down:
                    return Vector3.forward;


                case Side.Left:
                    return Vector3.right;


                case Side.Right:
                    return Vector3.left;


                default:
                    Debug.LogError("Oh no...");
                    break;

            }
            return Vector3.zero;

        }

        public void FinishInitialisation()
        {
            _initialised = true;
        }

        public void StartDeath()
        {
            if (_dying) return;

            _dying = true;
            _chunkObject.StartDeathTimer();
        }
        public void StopDeath()
        {
            _chunkObject.StopDeathTimer();
            _dying = false;
        }


        public Chunk FindNeighbour(Connection _connection)
        {
            foreach (Chunk c in _connectedChunks)
            {
                foreach (Connection con in c._connections)
                {
                    if (con._point == _connection._point) return c;
                }
            }
            return null;
        }


    }


}
