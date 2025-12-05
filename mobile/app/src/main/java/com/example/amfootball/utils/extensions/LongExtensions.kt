package com.example.amfootball.utils.extensions

import java.time.Instant
import java.time.LocalDate
import java.time.LocalDateTime
import java.time.ZoneId

/**
 * Converte um timestamp (valor em milissegundos) do tipo [Long] para um objeto [LocalDateTime].
 *
 * É uma função de extensão, o que permite chamá-la diretamente em qualquer variável Long
 * (ex: `dataMillis.toLocalDateTime()`).
 *
 * O método assume o fuso horário (Time Zone) padrão do sistema para realizar a conversão,
 * transformando os milissegundos UTC (época) para uma data e hora locais.
 *
 * @receiver O timestamp em milissegundos desde a época (epoch, 1 de Janeiro de 1970 UTC).
 * @return Um objeto [LocalDateTime] que representa a data e hora no fuso horário do sistema.
 */
fun Long.toLocalDateTime(): LocalDateTime {
    return Instant.ofEpochMilli(this)
        .atZone(ZoneId.systemDefault())
        .toLocalDateTime()
}

/**
 * Converte um timestamp (valor em milissegundos) do tipo [Long] para um objeto [LocalDate].
 *
 * Funciona de forma semelhante ao [toLocalDateTime], mas descarta a informação de horas,
 * minutos e segundos, retornando apenas a componente de Data (Ano, Mês, Dia).
 *
 * Útil para filtros ou visualizações onde o horário específico não é relevante.
 *
 * @receiver O timestamp em milissegundos desde a época (epoch).
 * @return Um objeto [LocalDate] que representa a data no fuso horário do sistema.
 */
fun Long.toLocalDate(): LocalDate {
    return Instant.ofEpochMilli(this)
        .atZone(ZoneId.systemDefault())
        .toLocalDate()
}