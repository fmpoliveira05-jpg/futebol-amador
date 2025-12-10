package com.example.amfootball.data.manager

import com.example.amfootball.data.local.CalendarPreference
import com.example.amfootball.data.repository.CalendarRepository
import javax.inject.Inject
import javax.inject.Singleton

/**
 * Gerenciador (Manager) de alto nível que orquestra as operações de calendário.
 *
 * Esta classe é responsável por coordenar a comunicação entre:
 * 1. [CalendarRepository]: Interage com o Calendar Provider do Android (adicionar/remover eventos).
 * 2. [com.example.amfootball.data.local.CalendarPreference]: Armazena e recupera o mapeamento entre o ID da partida (API) e o ID do evento (Android).
 *
 * Garante que a adição e remoção de eventos seja atômica: se o evento for criado/removido,
 * a associação local também deve ser salva/removida.
 *
 * @property calendarRepository O repositório para acesso ao sistema de calendário do Android.
 * @property calendarPreferences O data source local para o mapeamento de IDs.
 */
@Singleton
class CalendarManager @Inject constructor(
    private val calendarRepository: CalendarRepository,
    private val calendarPreferences: CalendarPreference
){

    /**
     * Verifica se um jogo específico (dado o seu ID da API) já foi adicionado ao calendário.
     *
     * A verificação é feita através da busca do ID do evento Android associado nas preferências locais.
     *
     * @param matchId O ID único da partida vindo da API.
     * @return `true` se a associação for encontrada, indicando que o evento foi adicionado.
     */
    fun isMatchAdded(matchId: String): Boolean {
        return calendarPreferences.getEventId(matchId) != null
    }

    /**
     * Adiciona uma partida como um novo evento no calendário do Android e armazena a sua referência localmente.
     *
     * **Operação Atômica de Criação:**
     * 1. Chama o [CalendarRepository.addEvent] para criar o evento no sistema.
     * 2. Se o evento for criado e um ID for retornado, o [matchId] e o novo [newId] são salvos em [CalendarPreference].
     *
     * @param matchId O ID da partida da API (Chave).
     * @param title O título do evento.
     * @param desc A descrição do evento.
     * @param loc O local do evento.
     * @param start O timestamp de início do evento (ms).
     * @param end O timestamp de fim do evento (ms).
     *
     * @return `true` se o evento foi adicionado com sucesso e a referência salva, `false` caso contrário (ex: erro de permissão).
     */
    fun addMatch(
        matchId: String,
        title: String,
        desc: String,
        loc: String,
        start: Long,
        end: Long
    ): Boolean {
        val newId = calendarRepository.addEvent(title, desc, loc, start, end)

        return if (newId != null) {
            calendarPreferences.saveEventId(matchId, newId)
            true
        } else {
            false
        }
    }

    /**
     * Remove uma partida do calendário do Android e deleta a sua referência local.
     *
     * **Operação Atômica de Remoção:**
     * 1. Recupera o ID do evento (Android ID) usando o [matchId].
     * 2. Se o ID for encontrado, chama [CalendarRepository.deleteEvent] para remover do sistema.
     * 3. Finalmente, remove a associação do [matchId] de [CalendarPreference].
     *
     * Esta operação é idempotente: chamar repetidamente não causará erro após a primeira exclusão.
     *
     * @param matchId O ID da partida da API a ser removida.
     */
    fun removeMatch(matchId: String) {
        val eventId = calendarPreferences.getEventId(matchId)

        if (eventId != null) {
            calendarRepository.deleteEvent(eventId)
            calendarPreferences.removeEventId(matchId)
        }
    }

    /**
     * Atualiza os detalhes de um evento de partida existente no calendário.
     *
     * **Fluxo de Atualização:**
     * 1. Recupera o ID do evento (Android ID) a partir do [matchId].
     * 2. Se o ID for encontrado, chama [CalendarRepository.updateEvent] com os novos detalhes.
     * Nota: O repositório deve lidar com a lógica de atualizar apenas os campos fornecidos.
     *
     * @param matchId O ID da partida da API que corresponde ao evento a ser atualizado.
     * @param title Novo título.
     * @param desc Nova descrição.
     * @param start Novo timestamp de início (ms).
     * @param end Novo timestamp de fim (ms).
     */
    fun updateMatch(matchId: String, title: String, desc: String, start: Long, end: Long) {
        val eventId = calendarPreferences.getEventId(matchId)
        if (eventId != null) {
            calendarRepository.updateEvent(eventId, title, desc, start, end)
        }
    }
}