using Defective.JSON;
using Fusion;
using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Utils
{
    public class JsonSerializeUtils
    {
        // https://doc-api.photonengine.com/en/fusion/v2/namespace_fusion.html
        // Not sure that the header is contained in the payload
        // Substarct the size of the payload by the header size and the the Offset(int) and NumberOdData(int) that we need to send
        public const int MaxChunckSize = (RpcAttribute.MaxPayloadSize - (NetworkId.SIZE + 2 + 2) - (sizeof(int) * 2)); // 512 - 8 - 8 (bytes)

        public static Dictionary<string, SplittedJsonFragment> SplitSerializedData(JSONObject json, int maxChunkSize = 492)
        {
            Dictionary<string, SplittedJsonFragment> listJsonFragment = new();
            SplittedJsonFragment jsonFragment = new SplittedJsonFragment(0, 0, 0, new(JSONObject.Type.Array));

            int jsonFragmentIdx = 0;
            int offset = 0;
            int iterations = 0;

            if (json.type != JSONObject.Type.Array)
            {
                Debug.Log("json.type isn't an Array");
            }

            foreach (JSONObject chunk in json)
            {
                // Get the size of the 
                string jsonString = json.list[offset].ToString();
                int jsonSize = System.Text.Encoding.UTF8.GetByteCount(jsonString);

                // Check if we cannot add data anymore
                if ((jsonFragment.Size + jsonSize) > maxChunkSize)
                {
                    // Reset jsonFragment
                    jsonFragmentIdx++;
                    jsonFragment = new SplittedJsonFragment(0, offset, iterations, new(JSONObject.Type.Array));
                    iterations = 0;
                }

                iterations++;

                // Actualize the data
                jsonFragment.Data.Add(json.list[offset]);
                jsonFragment.Size = System.Text.Encoding.UTF8.GetByteCount(jsonFragment.Data.ToString());
                jsonFragment.NumberOfData = iterations;

                offset++;

                listJsonFragment[jsonFragmentIdx.ToString()] = jsonFragment;
            }
            return listJsonFragment;
        }
    }

    #region Struct
    public struct SplittedJsonFragment
    {
        public int Size;
        public int Offset;
        public int NumberOfData;

        public JSONObject Data;

        // Constructor
        public SplittedJsonFragment(int size, int offset, int numberOfData, JSONObject data)
        {
            Size = size;
            Offset = offset;
            NumberOfData = numberOfData;

            Data = data;
        }
    }
    #endregion
}