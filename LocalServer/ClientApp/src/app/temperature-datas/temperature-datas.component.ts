import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { TemperatureData } from '../models/temperature-data';
import { TemperatureDataService } from '../services/temperature-data.service';

@Component({
  selector: 'app-temperature-datas',
  templateUrl: './temperature-datas.component.html',
  styleUrls: ['./temperature-datas.component.css']
})
export class TemperatureDatasComponent implements OnInit {

  temperatureDatas: Observable<TemperatureData[]>
  currentTemperatureData: Observable<TemperatureData[]>

  constructor(private temperatureDaraService: TemperatureDataService) { }

  ngOnInit() {
    this.load();
  }

  load() {
    this.currentTemperatureData = this.temperatureDaraService.getLast();
    this.temperatureDatas = this.temperatureDaraService.getLast20Data();
  }

  delete(id: number) {
    this.temperatureDaraService.deleteData(id).subscribe(() => {
      this.load();
    });
  }

}
