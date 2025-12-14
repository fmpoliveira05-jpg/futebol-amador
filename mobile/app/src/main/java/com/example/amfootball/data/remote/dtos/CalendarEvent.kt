package com.example.amfootball.data.remote.dtos

/**
 * Data Transfer Object (DTO) que representa um evento de calendário.
 *
 * Este DTO é usado para modelar eventos que podem ser exibidos num calendário,
 * sincronizados ou criados na aplicação (ex: um treino, um jogo agendado, uma reunião).
 *
 * @property title O título ou nome do evento (ex: "Jogo contra os Tigres FC").
 * @property description A descrição detalhada do evento (pode ser nula).
 * @property start O timestamp de início do evento, representado em milissegundos (Long).
 * @property end O timestamp de fim do evento, representado em milissegundos (Long).
 * @property location O nome ou endereço do local onde o evento irá ocorrer (pode ser nulo).
 */
data class CalendarEvent(
    val title: String,
    val description: String?,
    val start: Long,
    val end: Long,
    val location: String?
)