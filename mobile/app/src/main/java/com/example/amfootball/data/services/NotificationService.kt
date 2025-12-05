package com.example.amfootball.data.services

import android.Manifest
import android.app.PendingIntent
import android.content.Context
import android.content.Intent
import android.content.pm.PackageManager
import android.os.Build
import androidx.core.app.ActivityCompat
import androidx.core.app.NotificationCompat
import androidx.core.app.NotificationManagerCompat
import com.example.amfootball.MainActivity
import com.example.amfootball.R
import com.example.amfootball.utils.NotificationConst
import dagger.hilt.android.qualifiers.ApplicationContext
import javax.inject.Inject
import javax.inject.Singleton

/**
 * Serviço responsável por construir e exibir Notificações Push locais no sistema Android.
 *
 * Esta classe abstrai a complexidade do uso direto do [NotificationManagerCompat], tratando
 * da permissão de runtime (Android 13+) e da criação dos Intents para abrir a [MainActivity].
 *
 * É injetada como Singleton, garantindo uma única instância ao longo do ciclo de vida da aplicação.
 *
 * @property context O contexto da aplicação injetado pelo Hilt.
 */
@Singleton
class NotificationService @Inject constructor(
    @ApplicationContext private val context: Context
) {
    /**
     * Constrói e exibe uma notificação no canal de "Equipa" ([NotificationConst.TEAM_CHANNEL_ID]).
     *
     * Este método trata de:
     * 1. Criar o [PendingIntent] para abrir a [MainActivity] quando a notificação é clicada.
     * 2. Configurar o layout e prioridade da notificação (Prioridade Alta, ícone, título, texto).
     * 3. Verificar a permissão de runtime [Manifest.permission.POST_NOTIFICATIONS] no Android 13 (Tiramisu) ou superior,
     * retornando silenciosamente se a permissão não tiver sido concedida.
     * 4. Disparar a notificação para o sistema.
     *
     * @param title O título principal a ser exibido na notificação.
     * @param message O corpo principal da mensagem.
     *
     * @see NotificationCompat
     */
    fun showNotificationTeam(title: String, message: String) {
        val intent = Intent(context, MainActivity::class.java).apply {
            flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TASK
        }

        val pendingIntent: PendingIntent = PendingIntent.getActivity(
            context,
            0,
            intent,
            PendingIntent.FLAG_IMMUTABLE
        )

        val builder = NotificationCompat.Builder(context, NotificationConst.TEAM_CHANNEL_ID)
            .setSmallIcon(R.mipmap.ic_launcher_round)
            .setContentTitle(title)
            .setContentText(message)
            .setPriority(NotificationCompat.PRIORITY_HIGH)
            .setContentIntent(pendingIntent)
            .setAutoCancel(true)

        with(NotificationManagerCompat.from(context)) {
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
                if (ActivityCompat.checkSelfPermission(context, Manifest.permission.POST_NOTIFICATIONS) != PackageManager.PERMISSION_GRANTED) {
                    return
                }
            }

            notify(System.currentTimeMillis().toInt(), builder.build())
        }
    }
}