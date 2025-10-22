package com.example.amfootball.ui

import android.annotation.SuppressLint
import android.content.Context
import android.location.Location
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.layout.*
import androidx.compose.material3.Button
import androidx.compose.material3.Text
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.example.amfootball.data.Match
import com.example.amfootball.viewmodel.MainViewModel
import kotlinx.coroutines.launch
import androidx.compose.ui.platform.LocalContext
import androidx.lifecycle.viewmodel.compose.viewModel
import com.google.android.gms.location.LocationServices
import com.google.android.gms.maps.model.CameraPosition
import com.google.android.gms.maps.model.LatLng
import com.google.maps.android.compose.GoogleMap
import com.google.maps.android.compose.MapProperties
import com.google.maps.android.compose.Marker
import com.google.maps.android.compose.MarkerState
import com.google.maps.android.compose.rememberCameraPositionState

@SuppressLint("MissingPermission")
@Composable
fun MapScreenContainer(onBack: () -> Unit, viewModel: MainViewModel = viewModel()) {
    val coroutineScope = rememberCoroutineScope()
    var userLat by remember { mutableStateOf(41.1579) } // fallback coords
    var userLng by remember { mutableStateOf(-8.6291) }

    // Request permission
    val requestPermissionLauncher = rememberLauncherForActivityResult(ActivityResultContracts.RequestPermission()) { accepted ->
        // nothing here; we try to get location below if accepted
    }

    LaunchedEffect(Unit) {
        requestPermissionLauncher.launch(android.Manifest.permission.ACCESS_FINE_LOCATION)
        // try get last known location
        try {
            val ctx = LocalContext.current
            val client = LocationServices.getFusedLocationProviderClient(ctx)
            client.lastLocation.addOnSuccessListener { loc: Location? ->
                loc?.let {
                    userLat = it.latitude
                    userLng = it.longitude
                }
            }
        } catch (e: Exception) {
            // ignore fallback coords
        }
    }

    val matches by viewModel.matches.collectAsState()
    var nearbyMatches by remember { mutableStateOf<List<Match>>(emptyList()) }

    Column(modifier = Modifier.fillMaxSize()) {
        Box(modifier = Modifier.weight(1f)) {
            val cameraPositionState = rememberCameraPositionState {
                position = CameraPosition.fromLatLngZoom(LatLng(userLat, userLng), 12f)
            }
            GoogleMap(
                modifier = Modifier.fillMaxSize(),
                cameraPositionState = cameraPositionState,
                properties = MapProperties(isMyLocationEnabled = true)
            ) {
                // user marker
                Marker(
                    state = MarkerState(LatLng(userLat, userLng)),
                    title = "Você",
                    snippet = "Localização atual"
                )

                // match markers
                matches.forEach { m ->
                    Marker(
                        state = MarkerState(LatLng(m.location.lat, m.location.lng)),
                        title = "Partida: ${m.teamAId.substring(0,6)} vs ${m.teamBId.substring(0,6)}",
                        snippet = m.location.address ?: "Campo"
                    )
                }
            }
        }
        Row(modifier = Modifier.fillMaxWidth().padding(8.dp), horizontalArrangement = Arrangement.SpaceBetween) {
            Button(onClick = {
                nearbyMatches = viewModel.findNearbyMatches(userLat, userLng, radiusKm = 15.0)
            }) { Text("Procurar partidas próximas") }
            Button(onClick = {
                coroutineScope.launch { viewModel.scheduleRandomNearbyMatch(userLat, userLng) }
            }) { Text("Gerar partida próxima (demo)") }
            Button(onClick = onBack) { Text("Voltar") }
        }
        Spacer(modifier = Modifier.height(8.dp))
    }
}