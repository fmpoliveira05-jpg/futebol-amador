package com.example.amfootball

import android.Manifest
import android.app.NotificationChannel
import android.app.NotificationManager
import android.content.pm.PackageManager
import android.os.Build
import android.os.Bundle
import android.util.Log
import android.widget.Toast
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.activity.result.contract.ActivityResultContracts
import androidx.appcompat.app.AppCompatActivity
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.ui.platform.LocalContext
import androidx.core.content.ContextCompat
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.data.events.AppEvent
import com.example.amfootball.data.events.GlobalEventBus
import com.example.amfootball.navigation.MainNavigation
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.ui.theme.AMFootballTheme
import com.example.amfootball.utils.NotificationConst
import dagger.hilt.android.AndroidEntryPoint
import javax.inject.Inject

/**
 * A Atividade principal ([AppCompatActivity]) da aplicação e o ponto de entrada da UI.
 *
 * As responsabilidades principais incluem:
 * 1. Configurar o sistema de Injeção de Dependência ([AndroidEntryPoint]).
 * 2. Gerir o [MainNavigation] (Compose Navigation Host).
 * 3. Configurar canais de notificação e solicitar permissões de runtime essenciais.
 *
 * Nota: Esta Activity não gere o fluxo de notificação FCM diretamente; essa tarefa é delegada
 * ao [com.example.amfootball.data.services.PushNotificationService].
 */
@AndroidEntryPoint
class MainActivity : AppCompatActivity() {
    @Inject
    lateinit var globalEventBus: GlobalEventBus

    /**
     * Chamado quando a Activity é criada. Configura a UI e os serviços de sistema.
     */
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        createNotificationChannel()

        requestPermissions()

        enableEdgeToEdge()

        val startDestination = Routes.GeralRoutes.HOMEPAGE.route

        setContent {
            AMFootballTheme {
                val navController = rememberNavController()

                MainNavigation(
                    globalNavController = navController,
                    startDestination = startDestination
                )

                // OBSERVADOR DE EVENTOS GLOBAIS
                ObserveGlobalEvents(
                    navController = navController,
                    eventBus = globalEventBus
                )
            }
        }
    }

    @Composable
    fun ObserveGlobalEvents(navController: NavHostController,
                            eventBus: GlobalEventBus
    ) {
        val context = LocalContext.current

        LaunchedEffect(Unit) {
            eventBus.events.collect { event ->
                when (event) {
                    is AppEvent.TeamDeleted -> {
                        Toast.makeText(context, event.message, Toast.LENGTH_LONG).show()
                        navigateToHomePage(navController = navController)
                    }
                    is AppEvent.UserLoggedOut -> {
                        navigateToHomePage(navController = navController)
                    }
                }
            }
        }
    }

    private fun navigateToHomePage(navController: NavHostController) {
        navController.navigate(route = Routes.GeralRoutes.HOMEPAGE.route) {
            popUpTo(0) { inclusive = true }
            launchSingleTop = true
        }
    }

    /**
     * Cria e regista o canal de notificações para a equipa ([NotificationConst.TEAM_CHANNEL_ID]).
     *
     * O canal é configurado com [NotificationManager.IMPORTANCE_HIGH], o que permite
     * a exibição de notificações visuais intrusivas (heads-up) e som.
     * Esta função é executada apenas em dispositivos Android 8.0 (Oreo) ou superior.
     */
    private fun createNotificationChannel() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            val channelTeam = NotificationChannel(
                NotificationConst.TEAM_CHANNEL_ID,
                "Notificações de Equipa",
                NotificationManager.IMPORTANCE_HIGH
            )
            channelTeam.description = "Notificações eventos da equipa"

            val notificationManager = getSystemService(NOTIFICATION_SERVICE) as NotificationManager
            notificationManager.createNotificationChannel(channelTeam)
        }
    }

    /**
     * Verifica as permissões de runtime da aplicação e solicita-as ao utilizador.
     *
     * Permissões solicitadas:
     * 1. [Manifest.permission.ACCESS_FINE_LOCATION] (Opcional, se necessário para a app).
     * 2. [Manifest.permission.POST_NOTIFICATIONS] (Essencial para FCM a partir do Android 13/Tiramisu).
     */
    private fun requestPermissions() {
        val permissionsToRequest = mutableListOf<String>()

        if (ContextCompat.checkSelfPermission(
                this,
                Manifest.permission.ACCESS_FINE_LOCATION
            ) != PackageManager.PERMISSION_GRANTED
        ) {
            permissionsToRequest.add(Manifest.permission.ACCESS_FINE_LOCATION)
        }

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
            permissionsToRequest.add(Manifest.permission.POST_NOTIFICATIONS)
        }

        if (permissionsToRequest.isNotEmpty()) {
            requestPermissionLauncher.launch(permissionsToRequest.toTypedArray())
        }
    }

    /**
     * Callback para lidar com a resposta do utilizador ao pedido de permissões.
     *
     * Após o utilizador conceder ou negar as permissões, este launcher é ativado.
     */
    private val requestPermissionLauncher = registerForActivityResult(
        ActivityResultContracts.RequestMultiplePermissions()
    ) { permissions ->
        val notificationGranted = permissions[Manifest.permission.POST_NOTIFICATIONS] ?: false
        if (!notificationGranted) {
            Log.d("PERMISSIONS", "Notificações negadas")
        }
    }
}