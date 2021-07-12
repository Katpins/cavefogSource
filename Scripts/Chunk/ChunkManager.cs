using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;
namespace caveFog
{
    //this boi organises chunck placement and generation
    public class ChunkManager : MonoBehaviour
    {
        [SerializeField]
        private ChunkSettings _chunkSettings = null;

        [SerializeField]
        private ChunkObject _meshPrefab = null;

        [HideInInspector]
        [SerializeField]
        private Chunk _activeChunk = null;

        private Chunk _startChunk = null;

        public List<Chunk> _existingChunks = new List<Chunk>();

        private Queue<Chunk> _chunksToCreate = new Queue<Chunk>();

        [SerializeField]
        private PlayerData _player = null;

        [SerializeField]
        private List<Vector3> _megedVertecies = new List<Vector3>();

        public Action<Chunk> _enteredChunk;

        private Coroutine _chunkCreationCoroutine;

        private bool _creatingChunks = false;

        public int _seed;
        private System.Random _seedGen;

        private void Start()
        {
            if (_chunkSettings == null)
            {
                Debug.LogError("No chunk settings");
                return;
            }

            if (_meshPrefab == null)
            {
                Debug.LogError("No chunk mesh prefab");
                return;
            }


            if (Application.isEditor) _seedGen = new System.Random(_seed);
            else _seedGen = new System.Random(_player.CaveSeed);

            _existingChunks.Clear();

            _startChunk = CreateChunk();
            _activeChunk = _startChunk;

            StartCreatingChunk(_activeChunk);
            AddNeighbours(_activeChunk, true);
            //AddNeighbours(_activeChunk, true);
            //AddNeighbours(_existingChunks[_existingChunks.Count - 1]);

            //ConnectNewChunks();

            // AddNeighbours(_existingChunks[_existingChunks.Count - 1]);
            // AddNeighbours(_existingChunks[_existingChunks.Count - 1]);



            //_activeChunk._chunckInitialised += PlacePlayer;
            // int _chunks = 5;

            // for (int i = 0; i < _chunks; i++)
            // {
            //     if (i >= _existingChunks.Count) break;
            //     if (_existingChunks[i] != _activeChunk) AddNeighbours(_existingChunks[i]);
            // }


        }

        private void PlacePlayer(Chunk in_chunk)
        {
            _player.SetUpPlayer?.Invoke(in_chunk._wormPath[5], math.normalize(in_chunk._wormPath[8] - in_chunk._wormPath[5]));
            in_chunk._visited = true;
        }

        public void UnstuckPlayer()
        {
            _player.MovePlayer?.Invoke(_activeChunk._wormPath[5], math.normalize(_activeChunk._wormPath[8] - _activeChunk._wormPath[5]));
        }

        private void OnDisable()
        {
            _activeChunk = null;
            _enteredChunk -= EnteredChunk;
        }

        private void OnEnable()
        {

            _enteredChunk += EnteredChunk;
        }

        //here we tackle updates to chunk when we enter chunk
        public void EnteredChunk(Chunk _chunk)
        {
            //first time we spawn inside chunk doesnt count
            if (_chunk == _activeChunk) return;

            //Debug.Log("We entered A chunk!");

            //Chunk _activeChunk = _activeChunk;


            _chunk._visited = true;

            //destroy old
            for (int i = 0; i < _activeChunk._connectedChunks.Count; i++)
            {
                if (_activeChunk._connectedChunks[i] == _chunk) continue;

                if (_activeChunk._connectedChunks[i]._visited == false) continue;
                _activeChunk._connectedChunks[i].StartDeath();


                // for (int j = 0; j < _activeChunk._connectedChunks[i]._connectedChunks.Count; j++)
                // {
                //     if (_activeChunk._connectedChunks[i]._connectedChunks[j] == _activeChunk) continue;
                //     _activeChunk._connectedChunks[i]._connectedChunks[j].StartDeath();

                // }

            }

            //stop death
            for (int i = 0; i < _chunk._connectedChunks.Count; i++)
            {
                //if (_chunk._connectedChunks[i] != _activeChunk) AddNeighbours(_chunk._connectedChunks[i]);
                //if (_chunk._connectedChunks[i]._connectedChunks.Count == 1) AddNeighbours(_chunk._connectedChunks[i]);
                if (_chunk._connectedChunks[i]._dying) _chunk._connectedChunks[i].StopDeath();


                // for (int j = 0; j < _chunk._connectedChunks[i]._connectedChunks.Count; j++)
                // {
                //     if (_chunk._connectedChunks[i]._connectedChunks[j]._dying) _chunk._connectedChunks[i]._connectedChunks[j].StopDeath();
                // }

            }
            _activeChunk = _chunk;

            if (_activeChunk != _startChunk) AddNeighbours(_activeChunk);

        }
        // private void CreateChunkObject(Chunk in_chunk)
        // {
        //     var _newChunk = Instantiate(_meshPrefab);
        //     _newChunk.SetUp(in_chunk, this);

        //     _existingChunks.Add(in_chunk);

        // }

        private void AddNeighbours(Chunk in_chunk, bool _skippEntrance = false)
        {

            //
            //Debug.Log("Adding neighbours");
            // _neighbour;
            for (int i = 0; i < in_chunk._connections.Count; i++)
            {
                //dont make neighbou for entrance USED FOR START
                if (_skippEntrance && in_chunk._connections[i] == in_chunk._entrance) continue;

                //check if current connection already connected
                Chunk _neighbour = in_chunk.FindNeighbour(in_chunk._connections[i]);

                if (_neighbour == null)
                {
                    _neighbour = CreateNeighbourChunk(in_chunk, in_chunk._connections[i]);
                    //CreateChunkObject(_neighbour);

                    StartCreatingChunk(_neighbour);

                    //modify mesh to connect to previous one
                    //ConnectChunks(in_chunk, _neighbour, in_chunk._connections[i]);
                }
                //else if (_neighbour._dying) _neighbour.StopDeath();


            }
        }

        public void RemoveChunk(Chunk in_chunk)
        {
            _existingChunks.Remove(in_chunk);
        }

        //create first chunck
        private Chunk CreateChunk()
        {
            Chunk _newChunk = new Chunk(Vector3.zero, _chunkSettings, _seedGen.Next(-1000, 1000));

            //add exit
            _newChunk.AddConnection();

            return _newChunk;
        }
        //create connected chunk
        private Chunk CreateNeighbourChunk(Chunk in_chunk, Connection in_connection)
        {

            Chunk _newChunk = new Chunk(in_chunk, _chunkSettings, in_connection, _seedGen.Next(-1000, 1000));
            //add exit
            _newChunk.AddConnection();

            return _newChunk;
        }


        //preparting for chunk creation adding chunk to the creation queue
        private void StartCreatingChunk(Chunk in_chunk)
        {

            _chunksToCreate.Enqueue(in_chunk);

            if (!_creatingChunks) _chunkCreationCoroutine = StartCoroutine(CreateUpdateChunks());
        }

        //initialisation of chunk in the queue adding new chunks only when they are initialised
        private IEnumerator CreateUpdateChunks()
        {
            _creatingChunks = true;
            while (_chunksToCreate.Count != 0)
            {
                Chunk _newChunk = _chunksToCreate.Dequeue();

                var _newChunkObject = Instantiate(_meshPrefab);
                _newChunkObject.SetUp(_newChunk, this, _newChunk == _startChunk);

                yield return new WaitUntil(() => _newChunk._initialised);

                _existingChunks.Add(_newChunk);

                if (_newChunk == _startChunk)
                {
                    //_newChunk.
                    PlacePlayer(_activeChunk);
                }
            }
            _creatingChunks = false;
        }
        private void OnDrawGizmos()
        {
            if (_megedVertecies == null) return;

            Gizmos.color = Color.red;
            for (int i = 0; i < _megedVertecies.Count; i++)
            {
                Gizmos.DrawCube(_megedVertecies[i], Vector3.one * 0.2f);
            }
        }
    }
}
