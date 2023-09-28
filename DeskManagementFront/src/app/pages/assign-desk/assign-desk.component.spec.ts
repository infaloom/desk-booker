import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AssignDeskComponent } from './assign-desk.component';

describe('AssignDeskComponent', () => {
  let component: AssignDeskComponent;
  let fixture: ComponentFixture<AssignDeskComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AssignDeskComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AssignDeskComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
