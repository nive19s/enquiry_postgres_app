import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NewEnquiry } from './new-enquiry';

describe('NewEnquiry', () => {
  let component: NewEnquiry;
  let fixture: ComponentFixture<NewEnquiry>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NewEnquiry]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NewEnquiry);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
