using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using TMPro;
public class TestRelay : MonoBehaviour
{
    [TooltipAttribute("This does not include the host. If we are making a four player game, we need to specify 3.")]
    public int numberOfPlayer = 3;

    public TMP_InputField joinInput;
    public string joinCode;
    async void Start()
    {
        //Both host and client will be doing this.

        await UnityServices.InitializeAsync(); //Initializes services.

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);

        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync(); //We dont gotta use a google account or whater. Creates a new anon account for the user.
        
    }

    public void CaptureJoinCode()
    {
        joinCode = joinInput.text;
    }
    //For host
    public async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(numberOfPlayer);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode);
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
        
    }

    //For client.
    public async void JoinRelay(string _joinCode = "")
    {
        if(_joinCode == "")
        {
            _joinCode = joinCode;
        }
        try
        {
            Debug.Log("Joining " + _joinCode);
            await RelayService.Instance.JoinAllocationAsync(_joinCode);
            Debug.Log("Joined " + _joinCode);
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
        

    }
}
