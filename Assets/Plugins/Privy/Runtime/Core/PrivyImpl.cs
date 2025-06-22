using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Privy
{
    internal class PrivyImpl : IPrivy
    {

        private HttpRequestHandler _httpRequestHandler;

        private PlayerPrefsDataManager _playerPrefsDataManager;

        private AuthRepository _authRepository;

        private AuthDelegator _authDelegator;
    
        private InternalAuthSessionStorage _internalAuthSessionStorage;
        //References to other layers

        private TaskCompletionSource<bool> _initializationCompletionSource = new TaskCompletionSource<bool>();
        public Task InitializationTask => _initializationCompletionSource.Task;

        private WebViewManager _webViewManager;
        private EmbeddedWalletManager _embeddedWalletManager;
        
        // Analytics
        private IClientAnalyticsIdRepository _clientAnalyticsIdRepository;
        private IAnalyticsRepository _analyticsRepository;
        private IAnalyticsManager _analyticsManager;
        
        public bool IsReady => _authDelegator.CurrentAuthState != AuthState.NotReady; //Initialization is as Not Ready, the asynchronous methods update this state to be either authenticated or unauthenticated
        public ILoginWithEmail Email { get; }
        public ILoginWithOAuth OAuth { get; }

        private PrivyUser _user;

        public PrivyUser User
        {
            get
            {
                if (_authDelegator.CurrentAuthState != AuthState.Authenticated)
                {
                    return null;
                }
                return _user;
            }
        }

        public AuthState AuthState 
        {
            get 
            {
                return _authDelegator.CurrentAuthState;
            }
        }


        public PrivyImpl(PrivyConfig config)
        {
            // Synchronous parts of the initialization
            PrivyEnvironment.Initialize(config);
            PrivyLogger.Configure(config);
            
            //This bridge allows javascript to talk to a unity game object
            var bridge = GameObject.FindObjectOfType<WebViewManagerBridge>();

            _webViewManager = new WebViewManager(config, bridge);

            _playerPrefsDataManager = new PlayerPrefsDataManager();
            _clientAnalyticsIdRepository = new ClientAnalyticsIdRepository(_playerPrefsDataManager);
            _httpRequestHandler = new HttpRequestHandler(config, _clientAnalyticsIdRepository);

            //Dependency Injection
            _internalAuthSessionStorage = new InternalAuthSessionStorage(_playerPrefsDataManager);
            _authRepository = new AuthRepository(_httpRequestHandler);
            _authDelegator = new AuthDelegator(_authRepository, _internalAuthSessionStorage);
            _embeddedWalletManager = new EmbeddedWalletManager(_webViewManager, _authDelegator);
            _analyticsRepository = new AnalyticsRepository(_httpRequestHandler, _clientAnalyticsIdRepository);
            _analyticsManager = new AnalyticsManager(_analyticsRepository);


            Email = new LoginWithEmail(_authDelegator);
            OAuth = new LoginWithOAuth(_authDelegator);
            _user = new PrivyUser(_authDelegator, _embeddedWalletManager);
        }

        internal async Task InitializeAsync()
        {
            // Asynchronous part of the initialization, e.g., restoring session
            await _authDelegator.RestoreSession();

            // Mark initialization as complete
            _initializationCompletionSource.SetResult(true);
            
            // Fire initialize analytics event
            await _analyticsManager.LogEvent(AnalyticsEvent.SdkInitialize);
        }    


        public void SetAuthStateChangeCallback(Action<AuthState> callback)
        {
            _authDelegator.SetAuthStateChangeCallback(callback);
        }

        public void Logout()
        {
            _authDelegator.Logout();
            _clientAnalyticsIdRepository.ResetClientId();
        }
    }
}