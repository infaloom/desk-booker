import { TestBed } from '@angular/core/testing';

import { DeskService } from './desk.service';

describe('DeskService', () => {
  let service: DeskService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DeskService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
