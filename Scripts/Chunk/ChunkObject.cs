using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace caveFog
{
    //boi responsible for mesh generation
    public class ChunkObject : MonoBehaviour
    {
        private ChunkManager _manager = null;

        /////////Componets//////////////

        [SerializeField]
        private DeathTimer _deathTimer = null;

        [SerializeField]
        private MeshGenerator _generator = null;

        [SerializeField]
        private VoxelGenerator _voxelGenerator = null;

        [SerializeField]

        private PropManager _propManager = null;

        /////////Data//////////////

        [SerializeField]
        private Chunk _chunkData;

        [SerializeField]
        private MeshFilter _meshObject = null;

        public Mesh ChunkMesh { get => _meshObject.mesh; }

        [SerializeField]
        private MeshCollider _meshCollider = null;

        [SerializeField]
        private BoxCollider _boundsTrigger = null;

        private List<Vector3> _entranceVerts = new List<Vector3>();

        private Vector3[] _edgeVerts;

        public void SetUp(Chunk in_chunkData, ChunkManager in_manager, bool _sealEntrance = false)
        {
            _chunkData = in_chunkData;
            _manager = in_manager;

            transform.position = _chunkData._centerPos;

            _boundsTrigger.size = new Vector3(_chunkData._settings._widthWU, _chunkData._settings._heightWU, _chunkData._settings._lengthWU);

            _chunkData._chunkObject = this;

            _voxelGenerator.SealEntrance = _sealEntrance;

            GenerateVoxels();
        }

        private void GenerateVoxels()
        {
            if (_voxelGenerator == null)
            {
                Debug.LogError("oops! No worm controller!");
                return;
            }

            _voxelGenerator._VoxelsGenerated += StartGeneratingMesh;
            _voxelGenerator.SetUp(_chunkData);

        }

        private void StartGeneratingMesh()
        {
            _voxelGenerator._VoxelsGenerated -= StartGeneratingMesh;
            _generator._MeshGenDone += ApplyMesh;
            _generator.GenerateMesh(_chunkData);
        }


        private void ApplyMesh()
        {
            if (_meshObject == null)
            {
                Debug.LogError("No mesh Filter!");
                return;
            }
            if (_chunkData._voxelMap == null)
            {
                Debug.LogError("Missing voxel data!");
                return;
            }

            _generator._MeshGenDone -= ApplyMesh;

            if (_meshObject.mesh != null) _meshObject.mesh.Clear();

            _meshObject.sharedMesh = _generator.GetMesh();

            UpdateCollider();

            PlaceProps();

            //call event on Init Finish
            _chunkData.FinishInitialisation();

            //make edge verticies array for tests
            // Vector3[] _vertex = _meshObject.mesh.vertices;
            // _edgeVerts = new Vector3[_chunkData.EdgeVerticies.Length];

            // for (int i = 0; i < _chunkData.EdgeVerticies.Length; i++)
            // {
            //     _edgeVerts[i] = _vertex[_chunkData.EdgeVerticies[i]];
            // }

        }

        private void PlaceProps()
        {
            _propManager.SetUpProps(_chunkData);
        }

        public void UpdateMeshData()
        {
            _meshObject.mesh.RecalculateBounds();
            _meshObject.mesh.RecalculateNormals();
            _meshObject.mesh.RecalculateTangents();
        }

        public void ClearMeshData()
        {
            if (_meshObject.sharedMesh != null) _meshObject.sharedMesh.Clear();
        }

        public void UpdateCollider()
        {
            _meshCollider.sharedMesh = _meshObject.mesh;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                _manager.EnteredChunk(_chunkData);
            }
        }

        //starting a timer before killing chunck
        public void StartDeathTimer()
        {
            _deathTimer.TimerEnd += Die;
            _deathTimer.StartDeathTimer(_chunkData._visited ? _chunkData._settings._timeToDie : _chunkData._settings._timeToDieIfNotVisited);
        }

        public void StopDeathTimer()
        {
            _deathTimer.StopDeathTimer();
            _deathTimer.TimerEnd -= Die;
        }

        //destroying chunk and removeing all references
        private void Die()
        {
            //remove references
            for (int i = 0; i < _chunkData._connectedChunks.Count; i++)
            {
                _chunkData._connectedChunks[i]._connectedChunks.Remove(_chunkData);
            }

            //return props to the pool
            _propManager.RemoveProps();

            _manager.RemoveChunk(_chunkData);

            Destroy(this.gameObject);

        }

        private void OnDisable()
        {
            _deathTimer.TimerEnd -= Die;
        }
        private void OnDrawGizmos()
        {
            if (_chunkData != null)
            {
                Gizmos.DrawWireCube(_chunkData._centerPos, new Vector3(_chunkData._settings._widthWU, _chunkData._settings._heightWU, _chunkData._settings._lengthWU));
            }
        }
    }

}
