using System.Threading.Tasks;

using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

using UnityEngine;


public class RelayManager : MonoBehaviour
{
	private bool ready;
	private static RelayManager instance;

	private async void Start()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Destroy(gameObject);
			return;
		}

		DontDestroyOnLoad(this);

		if (!IsReady())
		{
			await UnityServices.InitializeAsync();
			AuthenticationService.Instance.SignedIn += () => { Debug.Log("Signed In"); };
			await AuthenticationService.Instance.SignInAnonymouslyAsync();

			ready = true;
		}
	}

	public bool IsReady()
	{
		return ready;
	}

	public async Task CreateRelay()
	{
		try
		{
			Allocation allocation = await RelayService.Instance.CreateAllocationAsync(Constants.maxPlayers - 1);
			string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
			Chat.Get.Log($"Creating relay with code {joinCode.ToUpper()}");
			GUIUtility.systemCopyBuffer = joinCode.ToUpper();

			RelayServerData relayServerData = new(allocation, "dtls");
			NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

			NetworkManager.Singleton.StartHost();
		}
		catch (RelayServiceException e)
		{
			Debug.Log(e);
		}
	}

	public async Task JoinRelay(string joinCode)
	{
		try
		{
			Chat.Get.Log($"Joining relay with code {joinCode.ToUpper()}");
			JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

			RelayServerData relayServerData = new(allocation, "dtls");
			NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

			NetworkManager.Singleton.StartClient();
		}
		catch (RelayServiceException e)
		{
			Debug.Log(e);
		}
	}
}