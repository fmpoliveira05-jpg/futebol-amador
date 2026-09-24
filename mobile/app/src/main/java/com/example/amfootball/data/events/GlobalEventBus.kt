package com.example.amfootball.data.events

import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.SharedFlow
import kotlinx.coroutines.flow.asSharedFlow
import javax.inject.Inject
import javax.inject.Singleton

/**
 * Um Barramento de Eventos Global (Global Event Bus) baseado em Kotlin SharedFlow.
 *
 * Esta classe é utilizada para comunicação assíncrona de eventos únicos (one-shot events)
 * entre diferentes camadas da aplicação (ex: Serviço -> ViewModel), sem acoplamento direto.
 *
 * É implementada como um Singleton para garantir que todos os componentes acessem a mesma instância
 * e o mesmo fluxo de eventos.
 */
@Singleton
class GlobalEventBus @Inject constructor() {

    /**
     * O fluxo mutável utilizado internamente para emitir eventos.
     *
     * - **SharedFlow:** É um hot flow, o que significa que ele começa a emitir valores
     * imediatamente, independentemente de haver coletores.
     * - **Replay:** O padrão é 0, o que é ideal para eventos (se um coletor começar a coletar
     * depois que um evento for emitido, ele não o receberá).
     * - **Buffer:** O padrão é 0, garantindo que a emissão e a coleta ocorram em sincronia
     * ou que a suspensão da emissão ocorra até que o buffer libere espaço.
     */
    private val _events = MutableSharedFlow<AppEvent>()

    /**
     * O fluxo público e somente leitura que os coletores utilizarão para subscrever os eventos.
     *
     * O uso de [.asSharedFlow] garante que o Mutável ([_events]) não seja exposto externamente,
     * impedindo que componentes externos emitam eventos diretamente, forçando o uso do método
     * de suspensão [emitEvent].
     */
    val events: SharedFlow<AppEvent> = _events.asSharedFlow()

    /**
     * Emite um novo evento no fluxo.
     *
     * Este método é uma função de suspensão (`suspend`) porque [MutableSharedFlow.emit]
     * é uma função de suspensão. Isso garante que a emissão será feita de forma segura
     * dentro de um contexto de coroutine.
     *
     * @param event A instância do evento (que deve ser uma subclasse de [AppEvent]) a ser distribuída.
     */
    suspend fun emitEvent(event: AppEvent) {
        _events.emit(event)
    }
}