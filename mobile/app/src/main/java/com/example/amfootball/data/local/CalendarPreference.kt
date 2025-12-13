package com.example.amfootball.data.local

import android.content.Context
import android.content.SharedPreferences
import dagger.hilt.android.qualifiers.ApplicationContext
import javax.inject.Inject

/**
 * Gerencia a persistência local de referências a eventos de calendário utilizando SharedPreferences.
 *
 * Esta classe é responsável por mapear o ID único de uma partida (fornecido pela API remota)
 * ao ID do evento correspondente criado no provedor de Calendário do Android.
 *
 * @property context O contexto da aplicação, injetado via Hilt, para garantir o ciclo de vida correto.
 */
class CalendarPreference @Inject constructor(
    @ApplicationContext context: Context
) {
    private val prefs: SharedPreferences =
        context.getSharedPreferences("calendar_prefs", Context.MODE_PRIVATE)

    /**
     * Salva a associação entre uma partida e um evento do calendário.
     *
     * Utiliza [SharedPreferences.Editor.apply] para gravação assíncrona,
     * evitando bloqueio da thread principal (UI Thread).
     *
     * @param matchId O ID único da partida (string), vindo da API de dados. Funciona como a Chave.
     * @param androidEventId O ID numérico (Long) retornado pelo CalendarProvider do Android. Funciona como o Valor.
     */
    fun saveEventId(matchId: String, androidEventId: Long) {
        prefs.edit().putLong(matchId, androidEventId).apply()
    }

    /**
     * Recupera o ID do evento do calendário associado a uma partida específica.
     *
     * @param matchId O ID único da partida (chave) para busca.
     * @return O ID do evento (Long) se encontrado, ou `null` se não houver registro salvo para esta partida.
     * A lógica interna converte o valor padrão -1 (não encontrado) para null.
     */
    fun getEventId(matchId: String): Long? {
        val id = prefs.getLong(matchId, -1)
        return if (id == -1L) {
            null
        } else {
            id
        }
    }

    /**
     * Remove a associação de uma partida específica.
     *
     * Deve ser chamado quando o usuário remove o evento do calendário através do app,
     * ou quando a partida deixa de existir.
     *
     * @param matchId O ID único da partida a ser removida das preferências.
     */
    fun removeEventId(matchId: String) {
        prefs.edit().remove(matchId).apply()
    }

    /**
     * Limpa TODOS os dados armazenados neste arquivo de preferências.
     *
     * Útil para operações de logout, limpeza de cache ou debug.
     * CUIDADO: Esta ação é irreversível e removerá todas as associações de eventos.
     */
    fun clearAll() {
        prefs.edit().clear().apply()
    }
}