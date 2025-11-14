package com.example.amfootball.utils.extensions

import java.time.Instant
import java.time.LocalDateTime
import java.time.ZoneId

/**
 * Este ficheiro permite adicionar um metodo que pode ser utilizado por qualquer variavel do
 * tipo Long
 * */

/**
 * Metodo que converte qualquer Long em DateTime
 * */
fun Long.toLocalDateTime(): LocalDateTime {
    return Instant.ofEpochMilli(this)
        .atZone(ZoneId.systemDefault())
        .toLocalDateTime()
}