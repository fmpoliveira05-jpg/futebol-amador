package com.example.amfootball

import android.Manifest
import android.os.Bundle
import android.util.Log
import android.widget.Toast
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.activity.result.contract.ActivityResultContracts
import androidx.activity.viewModels
import androidx.appcompat.app.AppCompatActivity
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.lifecycleScope
import androidx.lifecycle.repeatOnLifecycle
import com.example.amfootball.navigation.MainNavigation
import com.example.amfootball.ui.viewModel.MainViewModel
import dagger.hilt.android.AndroidEntryPoint
import kotlinx.coroutines.launch

/**
 * A Atividade principal da aplicação, atuando como o ponto de entrada do sistema Android.
 *
 * É responsável pela configuração inicial, pelo host da hierarquia de Views do Jetpack Compose
 * e pela gestão das permissões de runtime.
 *
 * Anotada com [@AndroidEntryPoint] para ativar a injeção de dependências do Hilt.
 *
 * Nota: Esta classe herda de [AppCompatActivity], que é a base recomendada para aplicações modernas.
 */
@AndroidEntryPoint
class MainActivity : AppCompatActivity() {
    private val viewModel: MainViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        // Permissões
        requestLocationPermission.launch(Manifest.permission.ACCESS_FINE_LOCATION)

        enableEdgeToEdge()

        setContent {
            MainNavigation()
        }
        observeNotifications()
    }

    private fun observeNotifications() {
        lifecycleScope.launch {
            repeatOnLifecycle(Lifecycle.State.STARTED) {
                viewModel.notificationFlow.collect { (title, message) ->
                    showUiNotification(title, message)
                }
            }
        }
    }


    private fun showUiNotification(title: String, message: String) {
        Log.d("SIGNALR_TEST", "6. UI a mostrar notificação agora!") // <--- LOG 6

        Toast.makeText(this, "$title\n$message", Toast.LENGTH_LONG).show()

        println("SIGNALR_DEBUG: Recebi: $title - $message")
    }

    /**
     * Launcher registrado para solicitar permissões de runtime (tempo de execução) ao utilizador.
     *
     * Este mecanismo moderno substitui o antigo `onRequestPermissionsResult`.
     *
     * Nota: O resultado pode ser processado aqui, mas o fluxo de negócio principal (ex: mostrar mensagem de erro)
     * é tipicamente gerido nos ViewModels e Composables.
     */
    private val requestLocationPermission = registerForActivityResult(
        ActivityResultContracts.RequestPermission()
    ) { granted ->
        // handled inside composables as well; kept here for completeness
    }
}