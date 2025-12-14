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

/**
 * Componente Composable que encapsula e exibe um mapa interativo utilizando a biblioteca osmdroid.
 *
 * Este componente utiliza [AndroidView] para integrar o widget View nativo do osmdroid ([MapView])
 * dentro da hierarquia de Compose. O mapa é configurado para exibir uma localização específica
 * marcada com um [Marker].
 *
 * **Dependência:** Requer a dependência e a configuração correta do osmdroid no projeto.
 *
 * @param latitude A latitude da localização a ser exibida e marcada no mapa.
 * @param longitude A longitude da localização a ser exibida e marcada no mapa.
 * @param modifier Modificador a ser aplicado ao container do mapa.
 */
@Composable
fun OsmMapView(
    latitude: Double,
    longitude: Double,
    modifier: Modifier = Modifier
) {
    val context = LocalContext.current

    val osmConfig = Configuration.getInstance()

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