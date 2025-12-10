package com.example.amfootball.data.repository

import android.content.ContentValues
import android.content.Context
import android.net.Uri
import android.provider.CalendarContract
import android.util.Log
import dagger.hilt.android.qualifiers.ApplicationContext
import java.util.TimeZone
import javax.inject.Inject
import javax.inject.Singleton

/**
 * Repositório responsável por gerenciar a interação direta com o Calendar Provider do Android.
 *
 * Esta classe abstrai a complexidade do uso de [android.content.ContentResolver] e fornece métodos
 * simplificados para adicionar, atualizar e remover eventos no calendário nativo do usuário.
 *
 * **Requisitos:**
 * Para que os métodos funcionem, o aplicativo deve solicitar e obter as seguintes permissões no AndroidManifest e em Runtime:
 * - `android.permission.READ_CALENDAR`
 * - `android.permission.WRITE_CALENDAR`
 *
 * @property context O contexto da aplicação (injetado via Hilt) usado para acessar o ContentResolver.
 */
@Singleton
class CalendarRepository @Inject constructor(
    @ApplicationContext private val context: Context
) {

    /**
     * Adiciona um novo evento ao calendário padrão do dispositivo.
     *
     * @param title O título do evento (ex: "NFL: Chiefs vs Eagles").
     * @param description A descrição detalhada ou notas do evento.
     * @param location O local do evento (pode ser um endereço ou nome do estádio).
     * @param startDate Timestamp de início em milissegundos (Epoch ms).
     * @param endDate Timestamp de término em milissegundos (Epoch ms).
     *
     * @return O ID único (Long) do evento criado no banco de dados do Android, ou `null` se:
     * - Não foi encontrado um calendário válido.
     * - Houve falha de permissão (SecurityException).
     * - Ocorreu erro na inserção.
     */
    fun addEvent(
        title: String,
        description: String,
        location: String,
        startDate: Long,
        endDate: Long
    ): Long? {
        val calendarId = getPrimaryCalendarId() ?: return null

        val values = ContentValues().apply {
            put(CalendarContract.Events.DTSTART, startDate)
            put(CalendarContract.Events.DTEND, endDate)
            put(CalendarContract.Events.TITLE, title)
            put(CalendarContract.Events.DESCRIPTION, description)
            put(CalendarContract.Events.EVENT_LOCATION, location)
            put(CalendarContract.Events.CALENDAR_ID, calendarId)
            put(CalendarContract.Events.EVENT_TIMEZONE, TimeZone.getDefault().id)
        }

        return try {
            val uri: Uri? = context.contentResolver.insert(CalendarContract.Events.CONTENT_URI, values)
            uri?.lastPathSegment?.toLongOrNull()
        } catch (e: SecurityException) {
            Log.e("Calendar", "Erro de permissão: ${e.message}")
            null
        }
    }

    /**
     * Atualiza os dados de um evento existente.
     *
     * Os parâmetros são opcionais (nullable). Apenas os campos não nulos serão atualizados.
     *
     * @param eventId O ID do evento no Android (retornado anteriormente por [addEvent]).
     * @param title Novo título (ou null para manter o atual).
     * @param description Nova descrição (ou null para manter a atual).
     * @param beginTime Novo horário de início em ms (ou null).
     * @param endTime Novo horário de término em ms (ou null).
     *
     * @return `true` se o evento foi atualizado com sucesso, `false` caso contrário (ex: evento não encontrado ou sem permissão).
     */
    fun updateEvent(
        eventId: Long,
        title: String?,
        description: String?,
        beginTime: Long?,
        endTime: Long?
    ): Boolean {
        val values = ContentValues()

        title?.let { values.put(CalendarContract.Events.TITLE, it) }
        description?.let { values.put(CalendarContract.Events.DESCRIPTION, it) }
        beginTime?.let { values.put(CalendarContract.Events.DTSTART, it) }
        endTime?.let { values.put(CalendarContract.Events.DTEND, it) }

        val updateUri = CalendarContract.Events.CONTENT_URI
        val selection = "${CalendarContract.Events._ID} = ?"
        val selectionArgs = arrayOf(eventId.toString())

        return try {
            val rows = context.contentResolver.update(updateUri, values, selection, selectionArgs)
            rows > 0
        } catch (e: SecurityException) {
            false
        }
    }

    /**
     * Remove um evento do calendário.
     *
     * @param eventId O ID do evento a ser excluído.
     * @return `true` se o evento foi deletado, `false` se não foi encontrado ou houve erro de permissão.
     */
    fun deleteEvent(eventId: Long): Boolean {
        val deleteUri = CalendarContract.Events.CONTENT_URI
        val selection = "${CalendarContract.Events._ID} = ?"
        val selectionArgs = arrayOf(eventId.toString())

        return try {
            val rows = context.contentResolver.delete(deleteUri, selection, selectionArgs)
            rows > 0
        } catch (e: SecurityException) {
            false
        }
    }

    /**
     * Helper privado para encontrar o ID do calendário principal do usuário.
     *
     * Realiza uma query na tabela de Calendários buscando preferencialmente aquele marcado como IS_PRIMARY.
     * Caso não encontre um marcado explicitamente, retorna o primeiro calendário disponível na lista.
     *
     * @return O ID do calendário (Long) ou null se nenhum for encontrado/acessível.
     */
    private fun getPrimaryCalendarId(): Long? {
        val projection = arrayOf(CalendarContract.Calendars._ID, CalendarContract.Calendars.IS_PRIMARY)

        try {
            context.contentResolver.query(
                CalendarContract.Calendars.CONTENT_URI, projection, null, null, null
            )?.use { cursor ->
                while (cursor.moveToNext()) {
                    val idIndex = cursor.getColumnIndex(CalendarContract.Calendars._ID)
                    val primaryIndex = cursor.getColumnIndex(CalendarContract.Calendars.IS_PRIMARY)

                    if (primaryIndex != -1 && cursor.getInt(primaryIndex) == 1) {
                        return cursor.getLong(idIndex)
                    }
                    return cursor.getLong(idIndex)
                }

            }
        } catch (e: SecurityException) {

        }

        return null
    }
}