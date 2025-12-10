package com.example.amfootball.ui

import android.content.Context
import androidx.compose.runtime.Composable
import androidx.compose.runtime.remember
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.viewinterop.AndroidView
import org.osmdroid.config.Configuration
import org.osmdroid.tileprovider.tilesource.TileSourceFactory
import org.osmdroid.util.GeoPoint
import org.osmdroid.views.MapView
import org.osmdroid.views.overlay.Marker

@Composable
fun OsmMapView(
    latitude: Double,
    longitude: Double,
    modifier: Modifier = Modifier
) {
    val context = LocalContext.current

    // --- CONFIGURAÇÃO DO OSMDROID ---
    val osmConfig = Configuration.getInstance()

    // CORREÇÃO: Em vez de PreferenceManager, usamos context.getSharedPreferences
    // "osmdroid" é o nome do ficheiro onde as configs serão salvas (podes dar o nome que quiseres)
    // Context.MODE_PRIVATE garante que só a tua app pode ler este ficheiro
    osmConfig.load(
        context,
        context.getSharedPreferences("osmdroid", Context.MODE_PRIVATE)
    )

    osmConfig.userAgentValue = context.packageName

    val location = remember(latitude, longitude) { GeoPoint(latitude, longitude) }

    AndroidView(
        modifier = modifier,
        factory = { ctx ->
            MapView(ctx).apply {
                setTileSource(TileSourceFactory.MAPNIK)
                setMultiTouchControls(true)
                controller.setZoom(18.0)
                controller.setCenter(location)
            }
        },
        update = { mapView ->
            mapView.controller.setCenter(location)

            mapView.overlays.clear()

            val marker = Marker(mapView)
            marker.position = location
            marker.setAnchor(Marker.ANCHOR_CENTER, Marker.ANCHOR_BOTTOM)
            marker.title = "Morada Selecionada"

            mapView.overlays.add(marker)
            mapView.invalidate()
        }
    )
}