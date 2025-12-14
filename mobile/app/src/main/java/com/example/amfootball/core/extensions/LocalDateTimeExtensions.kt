package com.example.amfootball.core.extensions

import com.example.amfootball.core.utils.Patterns
import java.time.LocalDateTime
import java.time.ZoneId
import java.time.format.DateTimeFormatter

/**
 * Converte um LocalDateTime para Long (Milissegundos desde Epoch).
 *
 * Utiliza o fuso horário predefinido do sistema (ZoneId.systemDefault()) para fazer a conversão.
 * Isto é ideal para guardar datas no Calendário do Android ou enviar para APIs que esperam timestamps locais.
 */
fun LocalDateTime.toLong(): Long {
    return this.atZone(ZoneId.systemDefault())
        .toInstant()
        .toEpochMilli()
}

/**
 * Converte o LocalDateTime para uma String formatada segundo o padrão global da App.
 * Exemplo: 25/12/2025 14:30
 */
fun LocalDateTime.toUiString(): String {
    val formatter = DateTimeFormatter.ofPattern(Patterns.DATE_TIME)
    return this.format(formatter)
}