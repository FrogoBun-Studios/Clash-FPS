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
	public async Task StartManager()
	{
		await UnityServices.InitializeAsync();

		AuthenticationService.Instance.SignedIn += () => { };

		await AuthenticationService.Instance.SignInAnonymouslyAsync();
	}

	public async void CreateRelay()
	{
		try
		{
			Allocation allocation = await RelayService.Instance.CreateAllocationAsync(7); //for 8 players
			string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
			Chat.Singleton.Log($"Creating relay with code {joinCode.ToUpper()}");
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

	public async void JoinRelay(string joinCode)
	{
		try
		{
			Chat.Singleton.Log($"Joining relay with code {joinCode.ToUpper()}");
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