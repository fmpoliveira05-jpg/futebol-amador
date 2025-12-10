package com.example.amfootball.data.events

import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.SharedFlow
import kotlinx.coroutines.flow.asSharedFlow
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class GlobalEventBus @Inject constructor() {
    private val _events = MutableSharedFlow<AppEvent>()

    val events: SharedFlow<AppEvent> = _events.asSharedFlow()

    suspend fun emitEvent(event: AppEvent) {
        _events.emit(event)
    }
}