package com.example.amfootball.data.events

/**
 * Interface selada (Sealed Interface) que define todos os eventos de alto nível
 * que podem ser transmitidos assincronamente através do [GlobalEventBus].
 *
 * Utilizar uma interface selada (ou classe selada) garante que todos os tipos de eventos
 * sejam conhecidos e definidos neste pacote. Isso é crucial para a segurança de tipos e
 * para permitir um tratamento exaustivo (com cláusulas 'when' que cobrem todos os casos)
 * nos coletores (Viewmodels/UI).
 */
sealed interface AppEvent {


    data class UpdateHomePage(val message: String) : AppEvent

    /**
     * Evento emitido quando o usuário é desconectado (logout).
     *
     * Utiliza 'data object' pois este evento não carrega estado (todos os eventos de logout
     * são iguais e não precisam de parâmetros no construtor).
     *
     * Este evento é crucial para:
     * - Navegar para a tela de Login.
     * - Limpar o estado global do aplicativo (cache, preferências, etc.).
     */
    data object UserLoggedOut : AppEvent
}