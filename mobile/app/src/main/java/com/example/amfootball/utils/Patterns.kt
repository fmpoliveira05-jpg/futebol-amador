package com.example.amfootball.utils

/**
 * Objeto que contém constantes de padrões de formatação de data e hora.
 *
 * Estas strings são usadas com formatadores como [java.time.format.DateTimeFormatter]
 * para garantir a consistência na apresentação de datas e horas em toda a aplicação.
 */
object Patterns {
    /** Padrão de formatação para data: Dia/Mês/Ano. Ex: 25/12/2025. */
    const val DATE = "dd/MM/yyyy"

    /** Padrão de formatação para hora: Hora:Minuto (24h). Ex: 14:30. */
    const val TIME = "HH:mm"

    /** Padrão de formatação combinada para Data e Hora. Ex: 25/12/2025 14:30. */
    const val DATE_TIME = "dd/MM/yyyy HH:mm"
}