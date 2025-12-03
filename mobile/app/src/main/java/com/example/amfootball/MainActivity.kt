package com.example.amfootball

import android.Manifest
import android.os.Bundle
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.activity.result.contract.ActivityResultContracts
import androidx.appcompat.app.AppCompatActivity
import com.example.amfootball.navigation.MainNavigation
import dagger.hilt.android.AndroidEntryPoint

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
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        // Solicita a permissão de localização no momento da criação da Atividade.
        // Necessário para funcionalidades como MatchMaker e Pitch Info.
        requestLocationPermission.launch(Manifest.permission.ACCESS_FINE_LOCATION)

        // Configura a aplicação para usar toda a área do ecrã, incluindo a área do sistema (Edge-to-Edge).
        enableEdgeToEdge()

        // Define o conteúdo da UI para ser o grafo de navegação principal do Compose.
        setContent {
            MainNavigation()
        }
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